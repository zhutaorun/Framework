using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using XLua;
using ClassPool;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GameFrame.AssetBundles
{
    [ExecuteInEditMode]
    public class AssetBundleManager : SingletonMBAuto<AssetBundleManager>
    {
        #region Variables
        //最大同时进行的加载器创建数量：WWW API同时加载回导致较高内存峰值和worker线程峰值
        private const int MAX_WWW_CREATE_NUM = 2;

        private const int MAX_CREATE_NUM = 20;

        //代替Unity的Manifest，同时包含了运行时ab缓存的信息
        private BundleManifest bundleManifest = null;

        private AssetsPathMapping assetsPathMapping = null;

        // asset缓存:给非公共ab包的asset提供逻辑层的复用
        private Dictionary<string, Object> assetsCaching = new Dictionary<string, Object>();

        // 1.Bundle请求：正在prosessing 或者等待prosessing 的Bundle请求
        private Dictionary<string, BundleRequester> assetBundleRequesting = new Dictionary<string, BundleRequester>();

        // 等待处理的Bundle请求
        private Queue<BundleRequester> assetBundleRequesterQueue = new Queue<BundleRequester>();

        //正在处理的Bundle请求

        private List<BundleRequester> prosessingAssetBundleRequester = new List<BundleRequester>();

        //2.Asset请求：正在prosessing 或者等待prosessing的Asset请求
        private Dictionary<string, AssetRequester> assetRequesting = new Dictionary<string, AssetRequester>();
        //等待处理的Asset请求
        private Queue<AssetRequester> assetRequesterQueue = new Queue<AssetRequester>();
        //正在处理的Asset请求
        private List<AssetRequester> prosessingAssetRequester = new List<AssetRequester>();
#if UNITY_EDITOR
        //3.EditorAsser的请求：正在prosessing或者等待prosessing的EditorAsset请求
        [BlackList]
        private Dictionary<string, EditorAssetAsynRequester> editorAssetRequesting = new Dictionary<string, EditorAssetAsynRequester>();

        // 等待就处理的EditorAsset请求
        [BlackList]
        private Queue<EditorAssetAsynRequester> editorAssetRequesterQueue = new Queue<EditorAssetAsynRequester>();

        //正在处理的EditorAsset请求
        [BlackList]
        private List<EditorAssetAsynRequester> prosessingEditorAssetRequester = new List<EditorAssetAsynRequester>();

#endif

        // 4.webAsset 请求：正在prosessing或者等待prosessing的Web资源请求
        Dictionary<string, ResourceWebRequester> webRequesting = new Dictionary<string, ResourceWebRequester>();

        //等待处理的资源请求
        Queue<ResourceWebRequester> webRequesterQueue = new Queue<ResourceWebRequester>();

        //正在处理的资源请求
        List<ResourceWebRequester> prosessingWebRequester = new List<ResourceWebRequester>();

        //逻辑层正在等待的ab加载异步句柄
        private List<BundleAsyncLoadHandle> prosessingAssetBundleAsyncLoader = new List<BundleAsyncLoadHandle>();
        //逻辑层正在等待的asset加载异步句柄
        private List<AssetAsyncLoadHandle> prosessingAssetAsyncLoader = new List<AssetAsyncLoadHandle>();
        // 正在等待的asset卸载的句柄，这里缓存延时一帧卸载
        private List<AssetAsyncLoadHandle> needDisposeAssetAsyncLoader = new List<AssetAsyncLoadHandle>();
        // 逻辑层正在等待的assetList加载异步句柄
        private List<AssetListAsyncLoadHandle> prosessingAssetListAsyncLoader = new List<AssetListAsyncLoadHandle>();


        private List<SceneAsyncLoadHandle> prosessingSceneAsyncLoader = new List<SceneAsyncLoadHandle>();
        private List<SceneAsyncLoadHandle> needDisposeSceneAsyncLoader = new List<SceneAsyncLoadHandle>();

#if UNITY_EDITOR
        //逻辑层正在等待的EditorAsset加载异步句柄
        [BlackList]
        private List<EditorAssetAsyncLoadHandle> prosessingEditorAssetAsyncLoader = new List<EditorAssetAsyncLoadHandle>();
        // 正在等待的EditorAsset卸载的句柄，这里缓存延时一帧卸载
        [BlackList]
        private List<EditorAssetAsyncLoadHandle> needDisposeEditorAssetAsyncLoader = new List<EditorAssetAsyncLoadHandle>();
        //逻辑层正在等待的EditorScene加载异步句柄
        [BlackList]
        private List<EditorSceneAsyncLoadHandle> prosessingEditorSceneAsyncLoader = new List<EditorSceneAsyncLoadHandle>();
        [BlackList]
        private List<EditorSceneAsyncLoadHandle> needDisposeEditorSceneAsyncLoader = new List<EditorSceneAsyncLoadHandle>();

#endif

        //请求失败的地址缓存
        private List<string> loadFaildAssetPaths = new List<string>();
        #endregion

        #region Initialize && Cleanup
        public IEnumerator Initialize()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                yield break;
            }
#endif
            assetsPathMapping = new AssetsPathMapping();
            bundleManifest = new BundleManifest();
            var pathMapRequest = RequestAssetBundleAsync(assetsPathMapping.AssetbundleName);
            var bundleManifestRequest = RequestAssetFileAsync(AssetBundleConfig.BundleManifestFileName);

            yield return pathMapRequest;
            var assetbundle = pathMapRequest.assetBundle;
            var mapContent = assetbundle.LoadAsset<TextAsset>(assetsPathMapping.AssetName);
            if (mapContent != null)
            {
                assetsPathMapping.Initialize(mapContent.text);
            }
            assetbundle.Unload(true);
            pathMapRequest.Dispose();

            yield return bundleManifestRequest;
            var strJson = bundleManifestRequest.text;
            bundleManifestRequest.Dispose();

            if (strJson.IsNullOrEmpty() == false)
            {
                bundleManifest = JsonUtility.FromJson<BundleManifest>(strJson);
                if (bundleManifest != null)
                {
                    bundleManifest.Initialize();
                }
            }
            InitializeResident(bundleManifest);
            yield break;
        }

        private void InitializeResident(BundleManifest manifest)
        {
            //设置所有公共包为常驻包
            var start = DateTime.Now;
            for (int i = 0; i < manifest.BundleList.Count; i++)
            {
                BundleManifest.AssetInfo info = manifest.BundleList[i];

                //说明：设置被依赖数量为1的AB包为常驻包的理由详情见BundleAsyncLoadHandle.cs那一大堆注释
                if (info.BeDependedOnCount >= 1)
                {
                    int bundleIndex = i;
                    SetAssetBundleResident(bundleIndex, true);
                }
                if (info.AssetBundleName.Contains(AssetBundleConfig.FontPathKey)
                    || info.AssetBundleName.Contains(AssetBundleConfig.AtlasPathKey)
                    || info.AssetBundleName.Contains(AssetBundleConfig.UiTexturePathKey)
                    || info.AssetBundleName.Contains(AssetBundleConfig.ShaderPathKey))
                {
                    int bundleIndex = i;
                    SetAssetBundleAlwaysNotUnload(bundleIndex, true);
                }

            }
            Debug.LogErrorFormat("AssetBundleResident Initialize use {0} ms", (DateTime.Now - start).TotalMilliseconds);
        }

        public IEnumerator Cleanup()
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                yield break;
            }
#endif

            //等待所有请求完成
            //要不是等待Unity很多版本都有各种bug
            yield return new WaitUntil(() =>
            {
                return !IsProsessRunning;
            });

            ClearAssetsCache();
            bundleManifest.CleanUp();
            yield break;
        }
        #endregion

        #region Load Methods
        #region Load AssetBundle


        /// <summary>
        /// AssetBundle:异步请求AssetBundle资源，AB是否缓存取决于是否设置为常驻包，处理依赖
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public BaseAssetBundleAsyncLoadHandle LoadAssetBundleAsync(string assetbundleName)
        {
            if (string.IsNullOrEmpty(assetbundleName))
            {
                Debug.LogError("AssetbundleName is null!");
                return null;
            }
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return new NullBundleAsyncLoadHandle(assetbundleName);
            }
#endif
            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            var loader = ClassObjectPool<BundleAsyncLoadHandle>.Get();
            if (bundleManifest != null)
            {
                string[] dependancies = bundleManifest.GetAllDependencies(assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    int dependanceIndex = bundleManifest.GetAssetBundleIndex(dependance);

                    if (IsAssetBundleLoaded(dependanceIndex) || assetBundleRequesting.ContainsKey(dependance))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
                    {
                        CreateBundleRequester(dependance);
                        //A 依赖于B,A对B持有引用
                        // int count = IncreateReferenceCount(dependanceIndex);
                    }
                }

                loader.Init(assetbundleName, bundleIndex, dependancies);
                prosessingAssetBundleAsyncLoader.Add(loader);
            }
            else
            {
                loader.Init(assetbundleName, bundleIndex, null);
                prosessingAssetBundleAsyncLoader.Add(loader);
            }
            CreateBundleRequester(assetbundleName);
            //加载器持有的引用：同一个ab能同时存在多个加载器，等待ab创建器完成
            IncreaseReferenceCount(bundleIndex);
            return loader;
        }

        /// <summary>
        /// AssetBundle:同步请求Assetbundle资源，AB是否缓存取决是否设置为常驻包，处理依赖
        /// 同步加载方式主要给一些暂时必须要用同步加载的资源使用，列如动画，一般不要使用
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public BaseAssetBundleAsyncLoadHandle LoadAssetBundle(string assetbundleName)
        {
            if (string.IsNullOrEmpty(assetbundleName))
            {
                Debug.LogError("AssetBundleName is null");
                return null;
            }
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return new NullBundleAsyncLoadHandle(assetbundleName);
            }
#endif
            if (bundleManifest != null)
            {
                string[] dependancies = bundleManifest.GetAllDependencies(assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    int dependanceIndex = bundleManifest.GetAssetBundleIndex(dependance);
                    if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName
                        && !IsAssetBundleLoaded(dependanceIndex))
                    {
                        string dependanceUrl = AssetBundlePath.GetAssetBundleDataPath(dependance);
                        AssetBundle dependanceAssetBundle = AssetBundle.LoadFromFile(dependanceUrl);
                        //A依赖于B,A对B持有引用
                        IncreaseReferenceCount(dependanceIndex);
                        if (dependanceAssetBundle != null)
                        {
                            BaseAssetBundleAsyncLoadHandle dependanceLoader = new BundleLoadHandle(dependance, dependanceAssetBundle);
                            AddAssetBundleCache(dependanceIndex, dependanceLoader.assetbundle);
                        }
                        else
                        {
                            Debug.LogErrorFormat("Load assetBundle at path:{0} failed!", dependance);
                        }
                    }
                }
            }

            int assetbundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            BaseAssetBundleAsyncLoadHandle loader = null;
            if (IsAssetBundleLoaded(assetbundleIndex))
            {
                loader = new BundleLoadHandle(assetbundleName, GetAssetBundleCache(assetbundleIndex));
            }
            else
            {
                string url = AssetBundlePath.GetAssetBundleDataPath(assetbundleName);
                AssetBundle assetBundle = AssetBundle.LoadFromFile(url);
                if (assetBundle != null)
                {
                    loader = new BundleLoadHandle(assetbundleName, assetBundle);
                    AddAssetBundleCache(assetbundleIndex, loader.assetbundle);
                }
                else
                {
                    Debug.LogErrorFormat("Load assetbundle at path:{0} failded!", assetbundleName);
                }

            }

            return loader;
        }


        public BundleRequester RequestAssetBundleAsync(string assetbundleName)
        {
            var creater = ClassObjectPool<BundleRequester>.Get();
            var url = AssetBundlePath.GetAssetBundleDataPath(assetbundleName);
            creater.Init(assetbundleName, url, true);
            assetBundleRequesting.Add(assetbundleName, creater);
            assetBundleRequesterQueue.Enqueue(creater);
            return creater;
        }
        #endregion

        #region Load Methods
        /// <summary>
        /// Asset:异步加载Scene资源，并加载Scene,默认缓存
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="loadMode"></param>
        /// <param name="callback"></param>
        /// <param name="autoDispose"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadSceneAsync(string assetPath, LoadSceneMode loadMode = LoadSceneMode.Single,
            Action<BaseAssetAsyncLoadHandle> callback = null, bool autoDispose = false)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("AssetPath is null !");
                return null;
            }
            string sceneName = GetSingleAssetName(assetPath);
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                if (!CheckAssetExist(assetPath))
                {
                    Debug.LogErrorFormat("No asset at asset path :{0}", assetPath);
                    AddToLoadFaildCatch(assetPath);
                    return null;
                }

                var editorLoader = ClassObjectPool<EditorSceneAsyncLoadHandle>.Get();
                editorLoader.Init(assetPath, sceneName, loadMode, callback, autoDispose);
                editorLoader.StartLoad();
                prosessingEditorSceneAsyncLoader.Add(editorLoader);
                return editorLoader;
            }
#endif
            string assetbundleName = null;
            string assetName = null;
            bool status = MapAssetPath(assetPath, out assetbundleName, out assetName);

            if (!status)
            {
                Debug.LogErrorFormat("No assetbundle at asset path：{0}", assetPath);
                AddToLoadFaildCatch(assetPath);
                return null;
            }
            var loader = ClassObjectPool<SceneAsyncLoadHandle>.Get();
            var assetbundleLoader = LoadAssetBundleAsync(assetbundleName);
            //Scene 加载器持有的引用，加载完成时会移除引用
            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            IncreaseReferenceCount(bundleIndex);
            loader.Init(assetName, assetbundleName, sceneName, loadMode, assetbundleLoader, callback, autoDispose);
            prosessingSceneAsyncLoader.Add(loader);
            return loader;
        }
        /// <summary>
        /// Asset:同步加载Scene资源,并加载Scene,默认缓存
        /// 同步加载方式主要给一些暂时必要要用同步加载的资源使用，例如动画，一般不要使用
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadScene(string assetPath, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("AssetPath is null!");
                return null;
            }

            string sceneName = GetSingleAssetName(assetPath);
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, loadMode);
                return new EditorAssetLoadHandle(null);
            }
#endif
            string assetbundleName = null;
            string assetName = null;
            bool stats = MapAssetPath(assetPath, out assetbundleName, out assetName);

            if (!stats)
            {
                Debug.LogErrorFormat("No assetbundle at asset path :{0}", assetPath);
                AddToLoadFaildCatch(assetPath);
                return null;
            }

            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            if (IsAssetBundleLoaded(bundleIndex))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, loadMode);
                return new AssetLoadHandle(null);
            }
            else
            {
                BaseAssetBundleAsyncLoadHandle bundleLoader = LoadAssetBundle(assetbundleName);
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, loadMode);
                return new AssetLoadHandle(null);
            }
        }
        #endregion

        #region Load Asset


        /// <summary>
        /// Asset:异步请求Asset资源，默认资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetType"></param>
        /// <param name="callback"></param>
        /// <param name="autoDispose"></param>
        /// <param name="catachAsset"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadAssetAsync(string assetPath, Type assetType,
            Action<BaseAssetAsyncLoadHandle> callback = null, bool autoDispose = false, bool catachAsset = true)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("AssetPath is null!");
                if (callback != null)
                {
                    callback.Invoke(null);
                }
                return null;
            }

            if (CheckInLoadFaildCatch(assetPath))
            {
                if (callback != null)
                {
                    callback.Invoke(null);
                }
                return null;
            }
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                if (!CheckAssetExist(assetPath))
                {
                    Debug.LogErrorFormat("no asset at asset path:{0}", assetPath);
                    AddToLoadFaildCatch(assetPath);
                    if (callback != null)
                    {
                        callback.Invoke(null);
                    }
                    return null;
                }

                var editorLoader = ClassObjectPool<EditorAssetAsyncLoadHandle>.Get();
                if (IsAssetLoaded(assetPath))
                {
                    editorLoader.Init(assetPath, GetAssetCache(assetPath), callback, autoDispose);
                }
                else
                {
                    editorLoader.Init(assetPath, assetType, callback, autoDispose, catachAsset);
                }

                //Requester默认缓存，Handle获取后处理缓存
                CreateEditorAssetRequester(assetPath, assetType);
                prosessingEditorAssetAsyncLoader.Add(editorLoader);
                return editorLoader;
            }
#endif
            string assetbundleName = null;
            string assetName = null;
            bool status = MapAssetPath(assetPath, out assetbundleName, out assetName);

            if (!status)
            {
                Debug.LogErrorFormat("No assetbundle at asset path: {0}", assetPath);
                AddToLoadFaildCatch(assetPath);

                if (callback != null)
                {
                    callback.Invoke(null);
                }
                return null;
            }

            var loader = ClassObjectPool<AssetAsyncLoadHandle>.Get();
            if (IsAssetLoaded(assetName))
            {
                loader.Init(assetName, null, GetAssetCache(assetName), callback, autoDispose);
            }
            else
            {
                var assetbundleLoader = LoadAssetBundleAsync(assetbundleName);
                //Asset加载器持有的引用，加载完成时会移除引用
                int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
                IncreaseReferenceCount(bundleIndex);

                loader.Init(assetName, assetbundleName, assetbundleLoader, callback, autoDispose, catachAsset);
            }
            prosessingAssetAsyncLoader.Add(loader);
            return loader;

        }

        /// <summary>
        /// Asset：同步请求Asset资源，默认缓存
        /// 同步加载方式主要给一些暂时必须要用同步加载的资源使用，例如动画，一般不要使用
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetType"></param>
        /// <param name="catchAsset"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadAsset(string assetPath, Type assetType, bool catchAsset = true)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("AssetPath is null!");
                return null;
            }
            if (CheckInLoadFaildCatch(assetPath))
            {
                return null;
            }

#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                if (!CheckAssetExist(assetPath))
                {
                    Debug.LogErrorFormat("No asset at asset path :{0}", assetPath);
                    AddToLoadFaildCatch(assetPath);
                    return null;
                }

                string path = AssetBundlePath.PackagePathToAssetsPath(assetPath);
                Object target = AssetDatabase.LoadAssetAtPath(path, assetType);
                return new EditorAssetLoadHandle(target);
            }
#endif
            string assetbundleName = null;
            string assetName = null;
            bool status = MapAssetPath(assetPath, out assetbundleName, out assetName);

            if (!status)
            {
                Debug.LogErrorFormat("No asset at asset path:{0}", assetPath);
                AddToLoadFaildCatch(assetPath);
                return null;
            }

            if (IsAssetLoaded(assetName))
            {
                return new AssetLoadHandle(GetAssetCache(assetName));
            }
            else
            {
                BaseAssetBundleAsyncLoadHandle bundleLoader = LoadAssetBundle(assetbundleName);
                AssetBundle assetBundle = bundleLoader.assetbundle;
                AssetLoadHandle loader = null;

                var assetFullPath = AssetBundlePath.PackagePathToAssetsPath(assetName);
                Object asset = assetBundle.LoadAsset(assetFullPath);

                if (asset != null)
                {
                    loader = new AssetLoadHandle(asset);
                    if (catchAsset)
                    {
                        AddAssetCache(assetName, loader.asset);
                    }
                }
                else
                {
                    Debug.LogErrorFormat(string.Format("Load asset at path :{0} failed!", assetPath));
                }
                return loader;
            }
        }

        /// <summary>
        /// 通过AssetPath List  异步加载Asset，一般用于预加载
        /// 不区分编辑器模式还是Bundle模式，在单个Loader中区分
        /// </summary>
        /// <param name="assetPaths"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadAssetsListAsync(List<string> assetPaths,
            Action<BaseAssetAsyncLoadHandle> callback = null)
        {
            var allAssetNames = assetPaths;
            List<BaseAssetAsyncLoadHandle> allLoaders = new List<BaseAssetAsyncLoadHandle>();

            for (int i = 0; i < allAssetNames.Count; i++)
            {
                var assetName = allAssetNames[i];
                if (!IsAssetLoaded(assetName))
                {
                    allLoaders.Add(LoadAssetAsync(assetName, typeof(Object), null, true));
                }
            }

            var loader = ClassObjectPool<AssetListAsyncLoadHandle>.Get();
            if (allLoaders.Count == 0)
            {
                loader.Init(callback);
            }
            else
            {
                loader.Init(allLoaders, callback);
            }
            prosessingAssetListAsyncLoader.Add(loader);
            return loader;
        }


        public BaseAssetAsyncLoadHandle LoadAssetsListAsync(List<BaseAssetAsyncLoadHandle> loaders,
            Action<BaseAssetAsyncLoadHandle> callback = null)
        {
            List<BaseAssetAsyncLoadHandle> allLoades = loaders;
            var loader = ClassObjectPool<AssetListAsyncLoadHandle>.Get();

            if (allLoades.Count == 0)
            {
                loader.Init(callback);
            }
            else
            {
                loader.Init(allLoades, callback);
            }

            prosessingAssetListAsyncLoader.Add(loader);
            return loader;
        }

        public BaseAssetAsyncLoadHandle LoadAssetsListByBundleNameAsync(string bundleName,
            Action<BaseAssetAsyncLoadHandle> callback = null)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return new NullAssetAsyncLoadHandle(bundleName, callback);
            }
#endif 
            List<string> allAssetNames = assetsPathMapping.GetAllAssetNames(bundleName);
            return LoadAssetsListAsync(allAssetNames, callback);
        }
        #endregion


        #region WebRequester

        /// <summary>
        /// 从服务器下载网页内容，需提供完成url,非AB(不计引用计数。不缓存)，Creater使用后记得回收
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ResourceWebRequester DownloadWebResourceAsync(string url)
        {
            var creater = ResourceWebRequester.Get();
            creater.Init(url, url, true);
            webRequesting.Add(url, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        /// <summary>
        /// 本地异步请求非Assetbundle资源，非AB(不计引用计数,不缓存)，Creater使用后记得回收
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="streamingAssetsOnly"></param>
        /// <returns></returns>
        public ResourceWebRequester RequestAssetFileAsync(string filePath, bool streamingAssetsOnly = true)
        {
            var creater = ResourceWebRequester.Get();
            string url = null;
            if (streamingAssetsOnly)
            {
                url = AssetBundlePath.GetStreamingAssetsFilePath(filePath);
            }
            else
            {
                url = AssetBundlePath.GetAssetBundleFileUrl(filePath);
            }
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }


        /// <summary>
        /// 从StreamingAssets通过www异步请求文件资源，非AB(不计引用计数，不缓存),Requester使用后记得回收
        /// </summary>
        /// <param name="filePath">相对于streamingAssets的路径，带后缀</param>
        /// <returns></returns>
        public ResourceWebRequester RequestFileFromStreamingAsync(string filePath)
        {
            var creater = ResourceWebRequester.Get();
            string url = null;
            url = AssetBundlePath.GetStreamingAssetsFilePath(filePath, false);
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }
        #endregion


        #region CreateRequester
        /// <summary>
        /// 创建Bundle加载器
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        private bool CreateBundleRequester(string assetbundleName)
        {
            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            if (IsAssetBundleLoaded(bundleIndex) || assetBundleRequesting.ContainsKey(assetbundleName))
            {
                return false;
            }

            var creater = ClassObjectPool<BundleRequester>.Get();
            var url = AssetBundlePath.GetAssetBundleDataPath(assetbundleName);
            creater.Init(assetbundleName, url);
            assetBundleRequesting.Add(assetbundleName, creater);
            assetBundleRequesterQueue.Enqueue(creater);
            //创建器持有的引用：创建器对于每个Ab来说时全局唯一的
            IncreaseReferenceCount(bundleIndex);
            return true;
        }

        /// <summary>
        /// 创建Asset加载器
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public bool CreateAssetRequester(string assetName, string assetbundleName)
        {
            //检查是否已经加载过
            if (IsAssetLoaded(assetName) || assetRequesting.ContainsKey(assetName))
            {
                return false;
            }

            var creater = ClassObjectPool<AssetRequester>.Get();
            creater.Init(assetbundleName, assetName);
            assetRequesting.Add(assetName, creater);
            assetRequesterQueue.Enqueue(creater);
            return true;
        }


        private bool CreateEditorAssetRequester(string assetName, Type assetType)
        {
#if UNITY_EDITOR
            //检查是否已经加载过，或者正在加载
            if (IsAssetLoaded(assetName) || editorAssetRequesting.ContainsKey(assetName))
            {
                return false;
            }

            var creater = ClassObjectPool<EditorAssetAsynRequester>.Get();

            creater.Init(assetName, assetType);
            editorAssetRequesting.Add(assetName, creater);
            editorAssetRequesterQueue.Enqueue(creater);
#endif
            return true;
        }
        #endregion
        #endregion

        #region Unload Methods
        public bool UnloadAssetBundle(string assetbundleName, bool unloadResident = false, bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
            if (String.IsNullOrEmpty(assetbundleName))
                return false;
            int assetbundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            int count = GetReferenceCount(assetbundleIndex);
            if (count > 0)
            {
                //存在引用，还是被需要的，不能卸载
                return false;
            }

            var assetbundle = GetAssetBundleCache(assetbundleIndex);
            var isResident = IsAssetBundleResident(assetbundleIndex);
            var isAssetBundleAlwaysNotUnload = IsAssetBundleAlwaysNotUnload(assetbundleIndex);
            if (!isAssetBundleAlwaysNotUnload && (!isResident || (isResident && unloadResident)))
            {
                if (assetbundle != null)
                {
                    assetbundle.Unload(unloadAllLoadedObjects);
                }

                RemoveAssetBundleCache(assetbundleIndex);
                if (unloadDependencies && bundleManifest != null)
                {
                    string[] dependencies = bundleManifest.GetAllDependencies(assetbundleIndex);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        var dependance = dependencies[i];
                        int dependanceIndex = bundleManifest.GetAssetBundleIndex(dependance);
                        if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
                        {
                            //解除对依赖项持有的引用
                            int dependanceCount = DecreaseReferenceCount(dependanceIndex);
                            if (dependanceCount <= 0)
                            {
                                UnloadAssetBundle(dependance, unloadResident, unloadAllLoadedObjects, false);
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 用于卸载无用AB包：如果该AB包还在使用，则卸载失败
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        /// <param name="unloadDependencies"></param>
        /// <returns></returns>
        public bool UnloadUnusedAssetBundle(string assetbundleName, bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            int count = GetReferenceCount(bundleIndex);
            if (count > 0)
            {
                return false;
            }
            return UnloadAssetBundle(assetbundleName, true, unloadAllLoadedObjects, unloadDependencies);
        }

        /// <summary>
        /// 用于卸载所有无用AB包：如果该AB包还在使用，则卸载失败
        /// </summary>
        /// <param name="unloadAllLoadedObjects"></param>
        /// <param name="unloadDependencies"></param>
        /// <returns></returns>
        public int UnloadAllUnusedResidentAssetBundles(bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return 0;
            }
#endif
            int unloadCount = 0;
            bool hasDoUnload = false;
            List<BundleManifest.AssetInfo> tmpAssetInfoList = new List<BundleManifest.AssetInfo>();

            do
            {
                hasDoUnload = false;
                tmpAssetInfoList.Clear();
                var iter = bundleManifest.LoadBundleList.GetEnumerator();
                while (iter.MoveNext())
                {
                    var bundleInfo = bundleManifest.GetAssetInfo(iter.Current);
                    if (!bundleInfo.IsAlwaysNotUnload && bundleInfo.RefCount <= 0)
                    {
                        tmpAssetInfoList.Add(bundleInfo);
                    }
                }
                for (int i = 0; i < tmpAssetInfoList.Count; i++)
                {
                    string toRemoveName = tmpAssetInfoList[i].AssetBundleName;
                    var result = UnloadAssetBundle(toRemoveName, true, unloadAllLoadedObjects, unloadDependencies);
                    if (result)
                    {
                        unloadCount++;
                        hasDoUnload = true;
                    }
                }
                tmpAssetInfoList.Clear();
            } while (hasDoUnload);

            return unloadCount;
        }

        /// <summary>
        /// 用于强制卸载Asset包，只会移除引用，不一定移除内存
        /// 需要调用 "Resources.UnloadUnusedAssets()"才会移除内存，但这个接口消耗比较大，一般只在Loading时调用
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool UnloadAsset(string assetName)
        {
            if (IsAssetLoaded(assetName))
            {
                RemoveAssetCache(assetName);
                return true;
            }
            return false;
        }
        #endregion

        #region Prosessing Methods
        public void Update()
        {
            OnProsessingBundleRequester();
            OnProsessingAssetBundleAsyncLoader();

            OnProsessingAssetRequester();
            OnProsessingAssetAsyncLoader();
            OnProsessingAssetListAsyncLoader();
            OnProsessingSceneAsyncLoader();

#if UNITY_EDITOR
            OnProsessingEditorAssetRequester(Time.deltaTime);
            OnProsessingEditorAssetAsyncLoader();
            OnProsessingEditorSceneAsyncLoader();
#endif
            OnProsessingWebRequester();
        }

        void OnProsessingBundleRequester()
        {
            for (int i = prosessingAssetBundleRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingAssetBundleRequester[i];
                creater.Update();
                if (creater.IsDone())
                {
                    prosessingAssetBundleRequester.RemoveAt(i);
                    assetBundleRequesting.Remove(creater.assetbundleName);
                    if (creater.noCache)
                    {
                        //无缓存，不计引用计数，Creater使用后由上层回收，所以这里不需要任何处理
                    }
                    else
                    {
                        //AB缓存
                        //说明：有错误也缓存下来，只不过资源为空
                        // 1.避免再次错误加载
                        // 2.如果不存下来加载器将无法判断什么时候结束
                        int bundleIndex = bundleManifest.GetAssetBundleIndex(creater.assetbundleName);
                        AddAssetBundleCache(bundleIndex, creater.assetBundle);

                        //解除创建器对AB持有的引用，一般创建器存在，则一定至少一个加载器等待并对该AB持有引用
                        int count = DecreaseReferenceCount(bundleIndex);
                        if (count <= 0)
                        {
                            UnloadAssetBundle(creater.assetbundleName, false, false, false);
                        }
                        creater.Dispose();
                    }
                }
            }

            int slotCount = prosessingAssetBundleRequester.Count;
            while (slotCount < MAX_CREATE_NUM && assetBundleRequesterQueue.Count > 0)
            {
                var creater = assetBundleRequesterQueue.Dequeue();
                creater.Start();
                prosessingAssetBundleRequester.Add(creater);
                slotCount++;
            }
        }

        void OnProsessingAssetBundleAsyncLoader()
        {
            for (int i = prosessingAssetBundleAsyncLoader.Count - 1; i > 0; i--)
            {
                var loader = prosessingAssetBundleAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    //解除加载器对AB的持有的引用
                    int bundleIndex = bundleManifest.GetAssetBundleIndex(loader.bundleName);
                    int count = DecreaseReferenceCount(bundleIndex);
                    if (count < 0)
                    {
                        UnloadAssetBundle(loader.bundleName, false, false, false);
                    }
                    prosessingAssetBundleAsyncLoader.RemoveAt(i);
                }
            }
        }

        void OnProsessingAssetRequester()
        {
            for (int i = prosessingAssetRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingAssetRequester[i];
                creater.Update();
                if (creater.IsDone())
                {
                    prosessingAssetRequester.RemoveAt(i);
                    assetRequesting.Remove(creater.assetName);
                    if (creater.noCache)
                    {
                        //无缓存，不计引用计数，Creater使用后由上层回收，所以这里不需要任何处理
                    }
                    else
                    {
                        //AB缓存
                        //说明：有错误也缓存下来，只不过资源为空
                        // 1.避免再次错误加载
                        // 2.如果不存下来加载器将无法判断什么时候结束
                        AddAssetCache(creater.assetName, creater.asset);
                        //TODO:验证
#if UNITY_EDITOR
                        //说明:在Editor 模拟时，Shader要重新指定
                        if (creater.asset != null)
                        {
                            var go = creater.asset as GameObject;
                            if (go != null)
                            {
                                var renderers = go.GetComponentsInChildren<Renderer>(true);
                                for (int j = 0; j < renderers.Length; j++)
                                {
                                    var mat = renderers[j].sharedMaterial;
                                    if (mat == null)
                                    {
                                        continue;
                                    }

                                    var shader = mat.shader;
                                    if (shader != null)
                                    {
                                        var shaderName = shader.name;
                                        mat.shader = Shader.Find(shaderName);
                                    }
                                }
                            }
                        }
#endif
                        creater.Dispose();
                    }
                }
            }

            int slotCount = prosessingAssetBundleRequester.Count;
            while (slotCount < MAX_CREATE_NUM && assetBundleRequesterQueue.Count > 0)
            {
                var creater = assetBundleRequesterQueue.Dequeue();
                creater.Start();
                prosessingAssetBundleRequester.Add(creater);
                slotCount++;
            }
        }

        void OnProsessingAssetAsyncLoader()
        {
            for (int i = 0; i < needDisposeAssetAsyncLoader.Count; i++)
            {
                var loader = needDisposeAssetAsyncLoader[i];
                needDisposeAssetAsyncLoader.Remove(loader);
                loader.Dispose();
            }

            for (int i = prosessingAssetAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    //解除Asset加载器对AB持有的引用
                    if (loader.bundleName != null)
                    {
                        int bundleIndex = bundleManifest.GetAssetBundleIndex(loader.bundleName);
                        int count = DecreaseReferenceCount(bundleIndex);
                        if (count <= 0)
                        {
                            UnloadAssetBundle(loader.bundleName, false, false, false);
                        }
                    }

                    prosessingAssetAsyncLoader.RemoveAt(i);
                    if (loader.autoDispose)
                        needDisposeAssetAsyncLoader.Add(loader);
                }
            }
        }

        void OnProsessingAssetListAsyncLoader()
        {
            for (int i = prosessingAssetListAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetListAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    prosessingAssetListAsyncLoader.RemoveAt(i);
                }
            }
        }

        void OnProsessingSceneAsyncLoader()
        {
            for (int i = 0; i < needDisposeSceneAsyncLoader.Count; i++)
            {
                var loader = needDisposeSceneAsyncLoader[i];
                needDisposeSceneAsyncLoader.Remove(loader);
                loader.Dispose();
            }

            for (int i = prosessingSceneAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingSceneAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    //解除Scene加载器对AB持有的引用
                    if (loader.bundleName != null)
                    {
                        int bundleIndex = bundleManifest.GetAssetBundleIndex(loader.bundleName);
                        int count = DecreaseReferenceCount(bundleIndex);
                        if (count <= 0)
                        {
                            UnloadAssetBundle(loader.bundleName, false, false, false);
                        }
                    }

                    prosessingSceneAsyncLoader.RemoveAt(i);
                    if (loader.autoDispose)
                        needDisposeSceneAsyncLoader.Add(loader);
                }
            }
        }

        void OnProsessingEditorAssetRequester(float deltaTime)
        {
#if UNITY_EDITOR
            for (int i = prosessingEditorAssetRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingEditorAssetRequester[i];
                creater.Update(deltaTime);
                if (creater.IsDone())
                {
                    prosessingEditorAssetRequester.RemoveAt(i);
                    editorAssetRequesting.Remove(creater.assetName);
                    if (creater.noCache)
                    {
                        //无缓存，Creater 使用后由上层回收，所以这里不需要做任何处理
                    }
                    else
                    {    //AB缓存
                        //说明：有错误也缓存下来，只不过资源为空
                        // 1.避免再次错误加载
                        // 2.如果不存下来加载器将无法判断什么时候结束
                        AddAssetCache(creater.assetName, creater.asset);

                        //说明:在Editor 模拟时，Shader要重新指定
                        var go = creater.asset as GameObject;
                        if (go != null)
                        {
                            var renderers = go.GetComponentsInChildren<Renderer>(true);
                            for (int j = 0; j < renderers.Length; j++)
                            {
                                var mat = renderers[j].sharedMaterial;
                                if (mat == null)
                                {
                                    continue;
                                }

                                var shader = mat.shader;
                                if (shader != null)
                                {
                                    var shaderName = shader.name;
                                    mat.shader = Shader.Find(shaderName);
                                }
                            }
                        }
                        creater.Dispose();
                    }
                }
            }

            int slotCount = prosessingAssetBundleRequester.Count;
            while (slotCount < MAX_CREATE_NUM && assetBundleRequesterQueue.Count > 0)
            {
                var creater = assetBundleRequesterQueue.Dequeue();
                creater.Start();
                prosessingAssetBundleRequester.Add(creater);
                slotCount++;
            }
#endif
        }

        void OnProsessingEditorAssetAsyncLoader()
        {
#if UNITY_EDITOR
            for (int i = 0; i < needDisposeEditorAssetAsyncLoader.Count; i++)
            {
                var loader = needDisposeEditorAssetAsyncLoader[i];
                needDisposeEditorAssetAsyncLoader.Remove(loader);
                loader.Dispose();
            }

            for (int i = prosessingEditorAssetAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingEditorAssetAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    prosessingEditorAssetAsyncLoader.RemoveAt(i);
                    if (loader.autoDispose)
                        needDisposeEditorAssetAsyncLoader.Add(loader);
                }
            }
#endif
        }

        void OnProsessingEditorSceneAsyncLoader()
        {
#if UNITY_EDITOR
            for (int i = 0; i < needDisposeEditorSceneAsyncLoader.Count; i++)
            {
                var loader = needDisposeEditorSceneAsyncLoader[i];
                needDisposeEditorSceneAsyncLoader.Remove(loader);
                loader.Dispose();
            }

            for (int i = prosessingEditorSceneAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingEditorSceneAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    prosessingEditorSceneAsyncLoader.RemoveAt(i);
                    if (loader.autoDispose)
                        needDisposeEditorSceneAsyncLoader.Add(loader);
                }
            }
#endif
        }

        void OnProsessingWebRequester()
        {
            for (int i = prosessingWebRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingWebRequester[i];
                creater.Update();
                if (creater.IsDone())
                {
                    prosessingWebRequester.RemoveAt(i);
                    webRequesting.Remove(creater.assetbundleName);

                    if (creater.noCache)
                    {
                        //无缓存，不计引用计数，Creater使用后由上层回收，所以这里不需要任何处理
                    }
                    else
                    {
                        creater.Dispose();
                    }
                }
            }

            int slotCount = prosessingWebRequester.Count;
            while (slotCount < MAX_CREATE_NUM && webRequesterQueue.Count > 0)
            {
                var creater = webRequesterQueue.Dequeue();
                creater.Start();
                prosessingWebRequester.Add(creater);
                slotCount++;
            }
        }
        #endregion

        #region BundleResident
        public void SetAssetBundleResident(string assetbundleName, bool resident)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return;
            }
#endif

            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            SetAssetBundleResident(bundleIndex, resident);
        }

        public void SetAssetBundleResident(int index, bool resident)
        {
            bundleManifest.SetAssetBundleResident(index, resident);
        }


        public bool IsAssetBundleResident(int index)
        {
            return bundleManifest.IsAssetBundleResident(index);
        }

        public void SetAssetBundleAlwaysNotUnload(int index, bool resident)
        {
            bundleManifest.SetAssetBundleAlwaysNotUnload(index, resident);
        }

        public bool IsAssetBundleAlwaysNotUnload(int index)
        {
            return bundleManifest.IsAssetBundleAlwaysNotUnload(index);
        }

        #endregion


        #region Cache Methods

        #region Bundle
        public bool IsAssetBundleLoaded(int index)
        {
            return bundleManifest.IsAssetBundleLoaded(index);
        }

        public AssetBundle GetAssetBundleCache(int index)
        {
            return bundleManifest.GetAssetBundleCache(index);
        }

        protected void RemoveAssetBundleCache(int index)
        {
            bundleManifest.RemoveAssetBundleCache(index);
        }

        protected void AddAssetBundleCache(int index,AssetBundle assetBundle)
        {
            bundleManifest.AddAssetBundleCache(index,assetBundle);
        }
        #endregion

        #region Asset
        public bool IsAssetLoaded(string assetName)
        {
            if(string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("AssetName is null");
                return false;
            }
            return assetsCaching.ContainsKey(assetName) || assetsCaching.ContainsKey(assetName.ToLower());
        }

        public Object GetAssetCache(string assetName)
        {
            Object target = null;
            assetsCaching.TryGetValue(assetName, out target);
            if(target == null)
            {
                assetsCaching.TryGetValue(assetName.ToLower(), out target);
            }
            return target;
        }

        public void AddAssetCache(string assetName,Object asset)
        {
            string path = assetName.ToLower();
            if(!IsAssetLoaded(path))
            {
                assetsCaching[path] = asset;
            }
        }

        public void RemoveAssetCache(string assetName)
        {
            string path = assetName.ToLower();
            if(!IsAssetLoaded(path))
            {
                assetsCaching.Remove(path);
            }
        }

        public void ClearAssetsCache()
        {
            assetsCaching.Clear();
        }
        #endregion
        #endregion

        #region Reference Count
        protected int GetReferenceCount(int bundleIndex)
        {
            return bundleManifest.GetReferenceCount(bundleIndex);
        }

        protected int IncreaseReferenceCount(int bundleIndex)
        {
            return bundleManifest.IncreaseReferenceCount(bundleIndex);
        }

        protected int DecreaseReferenceCount(int bundleIndex)
        {
            return bundleManifest.DecreaseReferenceCount(bundleIndex);
        }
        #endregion

        #region Help Methods
        /// <summary>
        /// 通过预存的资源路径判断资源是否存在，丢失资源的情况无法判断
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool CheckAssetExist(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
#if UNITY_EDITOR
            if(AssetBundleConfig.IsEditorMode)
            {
                return File.Exists(AssetBundlePath.GetFullPath(assetPath));
            }
#endif
            string assetbundleName = string.Empty;
            string assetName = string.Empty;
            return AssetBundleManager.Instance.MapAssetPath(assetPath,out assetbundleName,out assetName);
        }

        public void AddToLoadFaildCatch(string assetName)
        {
            assetName = assetName.ToLower();
            if(!loadFaildAssetPaths.Contains(assetName))
            {
                loadFaildAssetPaths.Add(assetName);
            }
        }

        public bool CheckInLoadFaildCatch(string assetName)
        {
            assetName = assetName.ToLower();
            return loadFaildAssetPaths.Contains(assetName);
        }


        public AssetRequester  GetAssetRequester(string assetName)
        {
            AssetRequester creater = null;
            assetRequesting.TryGetValue(assetName, out creater);
            return creater;
        }

        public EditorAssetAsynRequester GetEditorAssetRequester(string assetName)
        {
            EditorAssetAsynRequester creater = null;
#if UNITY_EDITOR
            editorAssetRequesting.TryGetValue(assetName,out creater);
#endif
            return creater;
        }

        public BundleRequester GetBundleRequester(string assetbundleName)
        {
            BundleRequester creater = null;
            assetBundleRequesting.TryGetValue(assetbundleName,out creater);
            return creater;
        }

        public bool MapAssetPath(string assetPath,out string assetbundleName,out string assetName)
        {
            assetPath = assetPath.ToLower();
            return assetsPathMapping.MapAssetPath(assetPath,out assetbundleName,out assetName);
        }

        private string GetSingleAssetName(string assetPath)
        {
            string assetName = assetPath;
            int pos = assetPath.IndexOf("/");
            if (pos != -1)
            {
                assetName = assetName.Substring(pos + 1);
                assetName = Path.GetFileNameWithoutExtension(assetName);
            }
            return assetName;
        }

        public BundleManifest CurBundleManifest
        {
            get
            {
                return bundleManifest;
            }
        }

        public bool IsProsessRunning
        {
            get
            {
#if UNITY_EDITOR
                return prosessingAssetBundleRequester.Count != 0
                || prosessingAssetBundleRequester.Count != 0
                || prosessingAssetAsyncLoader.Count != 0
                || prosessingAssetListAsyncLoader.Count != 0
                || prosessingEditorAssetAsyncLoader.Count != 0
                || prosessingWebRequester.Count != 0
                ;
#else
                return prosessingAssetBundleRequester.Count !=0
                || prosessingAssetBundleAsyncLoader.Count !=0
                || prosessingAssetAsyncLoader.Count !=0
                || prosessingAssetListAsyncLoader.Count !=0
                || prosessingWebRequester.Count !=0
                ;
#endif
            }
        }

        public string GetAssetBundleName(string assetName)
        {
            if (assetsPathMapping != null)
                return assetsPathMapping.GetAssetBundleName(assetName);
            return null;
        }
        #endregion

        #region Editor Debug
#if UNITY_EDITOR
        [BlackList]
        List<string> tempStringList = new List<string>();
        [BlackList]
        HashSet<string> tempStringSet = new HashSet<string>();

        [BlackList]
        public HashSet<string> GetAssetbundleResident()
        {
            tempStringSet.Clear();
            foreach(var one in bundleManifest.BundleList)
            {
                if(one !=null && one.IsResident)
                {
                    tempStringSet.Add(one.AssetBundleName);
                }
            }
            return tempStringSet;
        }

        [BlackList]
        public List<string> GetAssetBundleCaching()
        {
            tempStringList.Clear();
            foreach(var one in bundleManifest.LoadBundleList)
            {
                BundleManifest.AssetInfo info = bundleManifest.GetAssetInfo(one);
                if (info != null)
                    tempStringList.Add(info.AssetBundleName);
            }

            return tempStringList;
        }

        [BlackList]
        public Dictionary<string,BundleRequester> GetBundleRequesting()
        {
            return assetBundleRequesting;
        }

        [BlackList]
        public List<BundleRequester> GetProsessingBundleRequester()
        {
            return prosessingAssetBundleRequester;
        }

        [BlackList]
        public List<BundleAsyncLoadHandle> GetProsessingAssetBundleAsyncLoader()
        {
            return prosessingAssetBundleAsyncLoader;
        }

        [BlackList]
        public List<AssetAsyncLoadHandle> GetProsessingAssetAsyncLoader()
        {
            return prosessingAssetAsyncLoader;
        }
        [BlackList]
        public List<AssetListAsyncLoadHandle> GetProsessingAssetListAsyncLoader()
        {
            return prosessingAssetListAsyncLoader;
        }

        [BlackList]
        public int GetAssetCachingCount()
        {
            return assetsCaching.Count;
        }

        [BlackList]
        public Dictionary<string,List<string>> GetAssetsCaching()
        {
            var assetbundleDic = new Dictionary<string, List<string>>();
            List<string> assetNameList = null;

            var iter = assetsCaching.GetEnumerator();
            while(iter.MoveNext())
            {
                var assetName = iter.Current.Key;
                string assetbundleName = "";
                if(AssetBundleConfig.IsSimulateMode)
                {
                    assetbundleName = assetsPathMapping.GetAssetBundleName(assetName);
                }
                else
                {
                    assetbundleName = assetName;
                }

                assetbundleDic.TryGetValue(assetbundleName, out assetNameList);
                if(assetNameList == null)
                {
                    assetNameList = new List<string>();
                }

                assetNameList.Add(assetName);
                assetbundleDic[assetbundleName] = assetNameList; 
            }
            return assetbundleDic;
        }

        [BlackList]
        public int GetAssetbundleRefrenceCount(string assetbundleName)
        {
            int bundleIndex = bundleManifest.GetAssetBundleIndex(assetbundleName);
            return GetReferenceCount(bundleIndex);
        }

        [BlackList]
        public int GetAssetbundleDependenceiesCount(string assetbundelName)
        {
            string[] dependancies = bundleManifest.GetAllDependencies(assetbundelName);
            int count = 0;
            for(int i=0;i<dependancies.Length;i++)
            {
                var cur = dependancies[i];
                if(!string.IsNullOrEmpty(cur) && cur!= assetbundelName)
                {
                    count++;
                }
            }
            return count;
        }

        [BlackList]
        public List<string> GetAssetBundleRefrences(string assetbundleName)
        {
            List<string> refrence = new List<string>();
            tempStringList.Clear();
            foreach(var one in bundleManifest.LoadBundleList)
            {
                BundleManifest.AssetInfo info = bundleManifest.GetAssetInfo(one);
                if (info != null)
                    tempStringList.Add(info.AssetBundleName);
            }

            var cachingIter = tempStringList.GetEnumerator();
            while(cachingIter.MoveNext())
            {
                var curAssetbundleName = cachingIter.Current;
                if(curAssetbundleName == assetbundleName)
                {
                    continue;
                }
                string[] dependancies = bundleManifest.GetAllDependencies(curAssetbundleName);
                for(int i=0;i<dependancies.Length;i++)
                {
                    var dependance = dependancies[i];
                    if(dependance == assetbundleName)
                    {
                        refrence.Add(curAssetbundleName);
                    }
                }
            }

            var requestingIter = assetBundleRequesting.GetEnumerator();
            while (requestingIter.MoveNext())
            {
                var curAssetbundleName = requestingIter.Current.Key;
                if (curAssetbundleName == assetbundleName)
                {
                    continue;
                }

                string[] dependancies = bundleManifest.GetAllDependencies(curAssetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    if (dependance == assetbundleName)
                    {
                        refrence.Add(curAssetbundleName);
                    }
                }
            }
            return refrence;
        }

        [BlackList]
        public List<string> GetAssetLoaderRefrences(string assetbundleName)
        {
            List<string> refrences = new List<string>();
            var iter = prosessingAssetAsyncLoader.GetEnumerator();
            while (iter.MoveNext())
            {
                var curAssetbundleName = iter.Current.bundleName;
                var curLoader = iter.Current;
                if(curAssetbundleName == assetbundleName)
                {
                    refrences.Add(curAssetbundleName);
                }
            }
            return refrences;
        }

        [BlackList]
        public List<string> GetAssetBundleLoaderRefrences(string assetbundleName)
        {
            List<string> refrences = new List<string>();
            var iter = prosessingAssetBundleAsyncLoader.GetEnumerator();
            while(iter.MoveNext())
            {
                var curAssetbundleName = iter.Current.bundleName;
                var curLoader = iter.Current;
                if(curAssetbundleName== assetbundleName)
                {
                    refrences.Add(curLoader.Sequence.ToString());
                }
            }
            return refrences;
        }
#endif
#endregion
    }
}
