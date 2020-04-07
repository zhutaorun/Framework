using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace GameFrame.AssetBundles
{
    public class ResourcesManager : SingletonMBAuto<ResourcesManager>
    {
        #region Assets
        /// <summary>
        /// 异步加载Asset：回调形式
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">请求回调</param>
        /// <param name="catchAsset">是否缓存</param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadAssetAsync<T>(string assetPath, Action<T> callback, bool catchAsset = true) where T : Object
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("LoadAssetAsync Error :AssetPath is null !");
                return null;
            }

            Action<BaseAssetAsyncLoadHandle> handleCallback = (BaseAssetAsyncLoadHandle loader) =>
            {
                if (loader != null)
                {
                    T obj = loader.asset as T;
                    if (callback != null)
                        callback.Invoke(obj);
                }
                else
                {
                    if (callback != null)
                        callback.Invoke(null);
                }
            };

            if (!IsRuntime())
            {
                var editorLoader = new EditorAssetAsyncLoadHandle();
                T asset = EditorLoadAsset<T>(assetPath);
                editorLoader.Init(assetPath, asset, handleCallback, true);
                return editorLoader;
            }
            else
            {
                var tempLoader = AssetBundleManager.Instance.LoadAssetAsync(assetPath, typeof(T), handleCallback, true, catchAsset);
                return tempLoader;
            }
        }

        /// <summary>
        /// 异步加载Asset:协程形式（需要手动释放LoadHandle）
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="catchAsset">是否缓存</param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle CoLoadAssetAsync(string assetPath, Action<BaseAssetAsyncLoadHandle> callback = null, bool catchAsset = true)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("CoLoadAssetAsync Error : AssetPath is null or empty!");
                return null;
            }

            if (!IsRuntime())
            {
                var editorLoader = new EditorAssetAsyncLoadHandle();
                Object asset = EditorLoadAsset<Object>(assetPath);
                editorLoader.Init(assetPath, asset, callback, false);
                return editorLoader;
            }
            else
            {
                var tempLoader = AssetBundleManager.Instance.LoadAssetAsync(assetPath, typeof(Object), callback, false, catchAsset);
                return tempLoader;
            }
        }

        /// <summary>
        /// 同步加载Asset
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="catchAsset">是否缓存</param>
        /// <returns></returns>
        public T LoadAsset<T>(string assetPath, bool catchAsset = true) where T : Object
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("LoadAsset Error:AssetPath is null !");
                return null;
            }

            if (!IsRuntime())
            {
                return EditorLoadAsset<T>(assetPath);
            }

            BaseAssetAsyncLoadHandle loadHandle = AssetBundleManager.Instance.LoadAsset(assetPath, typeof(T), catchAsset);
            if (loadHandle != null)
            {
                return loadHandle.asset as T;
            }
            return null;
        }


        /// <summary>
        /// 异步加载Asset list
        /// </summary>
        /// <param name="assetPaths">资源路径</param>
        /// <param name="callback">请求回调</param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadAssetsListAsync(List<string> assetPaths, Action<BaseAssetAsyncLoadHandle> callback = null)
        {
            if (!IsRuntime())
            {
                Debug.LogError("LoadAssetsListAsync function only work in runtime!");
                return null;
            }
            return AssetBundleManager.Instance.LoadAssetsListAsync(assetPaths, callback);
        }

        /// <summary>
        /// 异步加载Asset list
        /// </summary>
        /// <param name="loaders">外部创建的加载器列表</param>
        /// <param name="callback">完成回调</param>
        /// <returns></returns>

        public BaseAssetAsyncLoadHandle LoadAssetsListAsync(List<BaseAssetAsyncLoadHandle> loaders, Action<BaseAssetAsyncLoadHandle> callback = null)
        {
            if (!IsRuntime())
            {
                Debug.LogError("LoadAssetsListAsync function only work in runtime!");
                return null;
            }
            return AssetBundleManager.Instance.LoadAssetsListAsync(loaders, callback);
        }

        /// <summary>
        /// 尝试从缓存中获取，必须预加载，否则为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T TryGetAssetFromCache<T>(string assetPath) where T : Object
        {
            if (!IsRuntime())
            {
                Debug.LogError("TryGetAssetFromCache function only work in runtime");
                return null;
            }

            if (!AssetBundleManager.Instance.IsAssetLoaded(assetPath))
            {
                Debug.LogErrorFormat("TryGetAssetFromCache Error:Asset is not catched at path:{0}", assetPath);
                return null;
            }
            else
            {
                Object asset = AssetBundleManager.Instance.GetAssetCache(assetPath);
                if (asset != null)
                {
                    return AssetBundleManager.Instance.GetAssetCache(assetPath) as T;
                }
                else
                {
                    Debug.LogErrorFormat("TryGetAssetFromCacheError:asset at path:{0} is null! :" + assetPath);
                    return null;
                }
            }
        }

        /// <summary>
        /// 通过预存的资源路径判断是否存在，丢失资源的情况无法判断
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public bool CheckAssetExist(string assetPath)
        {
            if (!IsRuntime())
            {
                return File.Exists(AssetBundlePath.GetFullPath(assetPath));
            }
            return AssetBundleManager.Instance.CheckAssetExist(assetPath);
        }

        /// <summary>
        /// 检查是否在加载失败的缓存列表
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool CheckInLoadFaildCatch(string assetPath)
        {
            if (AssetBundleManager.Instance == null)
            {
                return false;
            }
            return AssetBundleManager.Instance.CheckInLoadFaildCatch(assetPath);
        }
        #endregion

        #region Scene

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle CoLoadSceneAsync(string assetPath, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("CoLoadSceneAsyncError:AssetPath is null !");
                return null;
            }
            return AssetBundleManager.Instance.LoadSceneAsync(assetPath, loadMode, null, false);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public BaseAssetAsyncLoadHandle LoadScene(string assetPath, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if(string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("LoadSceneError:AssetPath is null !");
            }
            return AssetBundleManager.Instance.LoadScene(assetPath,loadMode);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public bool UnloadScene(string sceneName)
        {
            Scene unload_scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if(!unload_scene.isLoaded)
            {
                return false;
            }
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            if(op==null || op.isDone)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region ReadText
        public string ReadTextString(string assetPath)
        {
            TextAsset asset = null;
            if (!IsRuntime())
            {
                asset = EditorLoadAsset<TextAsset>(assetPath);
            }
            else
            {
                asset = LoadAsset<TextAsset>(assetPath, false);
            }

            if (asset == null)
            {
                Debug.LogErrorFormat("ReadTextString from path:{0} is null !", assetPath);
                return null;
            }

            string assetStr = asset.text;
            return assetStr;

        }

        public MemoryStream ReadTextStream(string assetPath)
        {
            TextAsset asset = null;
            if (!IsRuntime())
            {
                asset = EditorLoadAsset<TextAsset>(assetPath);
            }
            else
            {
                asset = LoadAsset<TextAsset>(assetPath,false);
            }

            if(asset == null)
            {
                Debug.LogErrorFormat("ReadTextStream from path:{0} is null!",assetPath);
                return null;
            }
            MemoryStream stream = new MemoryStream(asset.bytes);
            return stream;
        }
        #endregion


        #region Other
        /// <summary>
        /// 编辑器下加载Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private T EditorLoadAsset<T>(string assetName) where T:Object
        {
#if UNITY_EDITOR
            string path = AssetBundlePath.PackagePathToAssetsPath(assetName);
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path.ToLower());
            return asset;
#else
            return null;
#endif
        }

        private bool IsRuntime()
        {
#if UNITY_EDITOR
            return (Application.isPlaying && AssetBundleManager.Instance != null);
#else
            return true;
#endif
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// 清理资源：一般切换场景时调用
        /// </summary>
        public void Cleanup()
        {
            if(!IsRuntime())
            {
                Debug.LogError("Cleanup function only work in runtime!");
                return;
            }
            AssetBundleManager.Instance.ClearAssetsCache();
            AssetBundleManager.Instance.UnloadAllUnusedResidentAssetBundles();

            //清理完缓存把lua也清理了,Lua应该常驻或者清理完再次加载，这里直接再重新加载一次Lua文件
            //基于lua架构的内容
            //AssetBundleManager.Instance.LoadAssetsListByBundleNameAsync(XLuaManager.Instance.AssetbundleName);

        }
        #endregion
    }

}