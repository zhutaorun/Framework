namespace DSFramework.Function.Resource
{   
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameFrame;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using System;
    using UnityEngine.AddressableAssets;
    using UnityEngine.AddressableAssets.ResourceLocators;
    using System.Linq;
    using UnityEngine.SceneManagement;
    using System.IO;
    using Object = UnityEngine.Object;


    public class ResourceManager : ManagerBase, IResourceManager
    {
        #region 内部数据
        private bool isInitialize = false;


        /// <summary>
        /// 已经加载Asset对应实例化对象InstanceID列表
        /// key:assetpath
        /// value:instanceID
        /// </summary>
        private Dictionary<string, int> _loadedAssetInstanceCountDic = new Dictionary<string, int>();

        /// <summary>
        /// 已实例化对象对应的key
        /// key:instanceId
        /// value:assetPath
        /// </summary>
        private Dictionary<int, string> _objectInstanceIdKeyDic = new Dictionary<int, string>();


        /// <summary>
        /// 实例化对象池
        /// </summary>
        private InstancePool _instancePool = new InstancePool();

        /// <summary>
        /// 已加载/正在加载中资源名对应的句柄
        /// </summary>
        private Dictionary<string, AsyncOperationHandle> _handleCaches = new Dictionary<string, AsyncOperationHandle>();


        /// <summary>
        /// 正在进行加载状态中的资源的数量
        /// </summary>
        private int _loadingAssetCount = 0;

        /// <summary>
        /// 常驻内存的资源路径哈希集(主要操作是查找)
        /// </summary>
        private HashSet<string> _residentAssetsHashSet = new HashSet<string>();

        /// <summary>
        /// 是否加载正在进行
        /// </summary>
        public bool IsProseeRunuing
        {
            get
            {
                return _loadingAssetCount != 0;
            }
        }

        private Dictionary<string, SceneInstance> _scenenInstanceNameKeyDic = new Dictionary<string, SceneInstance>();
        #endregion


        #region 初始化/清理


        /// <summary>
        /// 是否初始化
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            return isInitialize;
        }

        /// <summary>
        /// 初始化(回调模式)
        /// </summary>
        /// <param name="completedCallback"></param>
        public void InitializeAsync(Action completedCallback)
        {
            Addressables.InitializeAsync().Completed += (AsyncOperationHandle<IResourceLocator> asyncOperationHandle) =>
            {
                UnityEngine.Debug.Log("Addressable初始化完成");
                isInitialize = asyncOperationHandle.IsDone;
                if (!isInitialize)
                {
                    Debug.LogError("ResourceManager initialize error!");
                }
                completedCallback?.Invoke();
                Addressables.Release(asyncOperationHandle);
            };
        }

        /// <summary>
        /// 清理所有(慎用)
        /// </summary>
        /// <returns></returns>
        public IEnumerator CoCleanup()
        {
            Debug.Log("CoCleanup 清理资源");
            //等待所有请求完成
            //要是不等待Unity很多版本都有各种Bug
            yield return new WaitUntil(() =>
            {
                return !IsProseeRunuing;
            });

            _instancePool.ClearAll();

            //卸载掉所有常驻资源之外的资源
            foreach (var item in _handleCaches)
            {
                if (!_residentAssetsHashSet.Contains(item.Key))
                {
                    Addressables.Release(item.Value);
                    if (_spriteCache.TryGetValue(item.Key, out SpriteAtlas spriteAtlas))
                    {
                        spriteAtlas.Release();
                        _spriteCache.Remove(item.Key);
                    }
                }
            }

            _handleCaches.Clear();
            _loadedAssetInstanceCountDic.Clear();
            _objectInstanceIdKeyDic.Clear();

            //清理完缓存把Lua也清理了,Lua应该常驻或清理完再次加载，这里直接再重新加载一次LUA文件
            yield return CoPreLoadAssetsByLabel<TextAsset>("LuaScripteAssets");
        }


        /// <summary>
        /// 清理
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("Cleanup 清理资源");
            _instancePool.ClearAll();

            //卸载掉所有常驻资源之外的资源
            foreach (var item in _handleCaches)
            {
                if (!_residentAssetsHashSet.Contains(item.Key))
                {
                    Addressables.Release(item.Value);
                }
            }

            _handleCaches.Clear();
            _loadedAssetInstanceCountDic.Clear();
            _objectInstanceIdKeyDic.Clear();
        }
        #endregion

        #region 实例化/预实例化/回收对象
        /// <summary>
        /// 实例化对象(回调模式)
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="active">是否激活（默认true）</param>
        /// <param name="autoUnload"></param>
        public void InstantiateAsync(string assetPath, Action<GameObject> callback, bool active = true, bool autoUnload = false)
        {
            if (!_handleCaches.ContainsKey(assetPath))
            {
                //未加载过此资源
                LoadAssetAsync<GameObject>(assetPath, (obj) =>
                {
                    if (obj != null)
                    {
                        InternalInstantiate(assetPath, callback, active);
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                }, autoUnload);
            }
            else
            {
                var handle = _handleCaches[assetPath];
                //已加载此资源且加载完成
                if (handle.IsDone)
                {
                    InternalInstantiate(assetPath, callback, active);
                }
                else
                {
                    //已加载此资源但正在加载
                    handle.Completed += (result) =>
                    {
                        InternalInstantiate(assetPath, callback, active);
                    };
                }
            }
        }

        /// <summary>
        /// 实例化对象(协程+回调模式,用于模拟同步)
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="active">是否激活（默认true）</param>
        /// <param name="autoUnload"></param>
        public IEnumerator CoInstantiateAsync(string assetPath, Action<GameObject> callback, bool active = true)
        {
            if (!_handleCaches.ContainsKey(assetPath))
            {
                //未加载过此资源
                yield return LoadAssetAsync<GameObject>(assetPath, null);
                InternalInstantiate(assetPath, callback, active);
            }
            else
            {
                var handle = _handleCaches[assetPath];
                //已加载此资源且加载完成
                if (handle.IsDone)
                {
                    InternalInstantiate(assetPath, callback, active);
                }
                else
                {
                    //已加载此资源但正在加载
                    yield return handle;
                    InternalInstantiate(assetPath, callback, active);
                }
            }
        }

        /// <summary>
        /// 回收某个对象实例入对象池
        /// </summary>
        /// <param name="instanceObject"></param>
        /// <param name="forceDestroy"></param>
        public void Recycle(GameObject instanceObject, bool forceDestroy = false)
        {
            if (instanceObject == null)
            {
                return;
            }

            if (_objectInstanceIdKeyDic.ContainsKey(instanceObject.GetInstanceID()))
            {
                _instancePool.Recycle(_objectInstanceIdKeyDic[instanceObject.GetInstanceID()], instanceObject, forceDestroy);
                _loadedAssetInstanceCountDic[_objectInstanceIdKeyDic[instanceObject.GetInstanceID()]]--;
                _objectInstanceIdKeyDic.Remove(instanceObject.GetInstanceID());
            }
            else
            {
                Debug.LogErrorFormat("此模块不回收不是从这个模块实例化出去的对象:{0}", instanceObject.name);
            }
        }


        /// <summary>
        /// 内部实例化对象
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="active"></param>
        private void InternalInstantiate(string assetPath, Action<GameObject> callback, bool active = true)
        {
            //实例化对象
            GameObject result = _instancePool.Get(assetPath);
            GameObject invokeResult;
            if (result == null)
            {
                if (!_handleCaches.ContainsKey(assetPath))
                {
                    Debug.LogErrorFormat("该资源已被清除：{0}", assetPath);
                    return;
                }
                invokeResult = _handleCaches[assetPath].Result as GameObject;
                if (invokeResult == null)
                {
                    Debug.LogErrorFormat("类型转换出了问题:{0}  path ={1}", _handleCaches[assetPath].Result, assetPath);
                }
                invokeResult = GameObject.Instantiate(invokeResult);
            }
            else
            {
                invokeResult = result as GameObject;
                if (invokeResult == null)
                {
                    Debug.LogErrorFormat("类型转换出了问题:{0}  path={1}", result.name, assetPath);
                }
            }

            //初始化对象
            InitInst(invokeResult, active);

            //对象信息存储
            _objectInstanceIdKeyDic[invokeResult.GetInstanceID()] = assetPath;
            _loadedAssetInstanceCountDic[assetPath]++;

            callback?.Invoke(invokeResult);
        }


        /// <summary>
        /// 内部用初始化实例对象
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="active"></param>
        private void InitInst(GameObject inst, bool active = true)
        {
            if (inst != null)
            {
                if (active)
                    inst.SetActive(true);
                else if (inst.activeSelf)
                    inst.SetActive(false);
            }
        }

        /// <summary>
        /// 预实例化对象:提供实例化个数，预先缓存好相应的
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="instCount"></param>
        /// <param name="callback"></param>
        public void PreInstantiateAsync(string assetPath, int instCount, Action callback)
        {
            LoadAssetAsync<GameObject>(assetPath, (GameObject obj) =>
            {
                for (int i = 0; i < instCount; i++)
                {
                    GameObject gameoObject = GameObject.Instantiate(obj);
                    _instancePool.Recycle(assetPath, gameoObject);
                }
                callback.Invoke();
            });
        }


        /// <summary>
        /// 预实例化对象:提供实例个数，预先缓存好相应的GameOject(协程模式)
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="instCount">实例化个数</param>
        /// <param name="callback">回调函数</param>
        /// <returns></returns>
        public AsyncOperationHandle CoPreInstantiateAsync(string assetPath, int instCount, Action callback)
        {
            var handle = LoadAssetAsync<GameObject>(assetPath, (GameObject obj) =>
            {
                for (int i = 0; i < instCount; i++)
                {
                    GameObject gameoObject = GameObject.Instantiate(obj);
                    _instancePool.Recycle(assetPath, gameoObject);
                }
                callback.Invoke();
            });
            return handle;
        }

        /// <summary>
        /// 预实例化某个label的全部对象：将某个label的所有资源实例化一个对象后放入池中缓存
        /// </summary>
        /// <param name="labelName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator CoPreInstantiateByLabel(string labelName, Action callback)
        {
            List<string> assetList = GetKeysListByLabel(labelName);
            for (int i = 0; i < assetList.Count; i++)
            {
                yield return CoPreInstantiateAsync(assetList[i], 1, null);
            }
            callback?.Invoke();
        }

        #endregion

        #region 预加载资源/资源缓存
        /// <summary>
        /// 预加载一个资源(回调模式)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="callback"></param>
        public void PreLoadAssetAsync<T>(string assetPath, Action<T> callback = null) where T : class
        {
            LoadAssetAsync<T>(assetPath, (obj) =>
            {
                callback?.Invoke(obj);
            });
        }

        /// <summary>
        /// 预加载一个资源(协程模式)(有同步加载需求时候使用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public IEnumerator CoPreLoadAsset<T>(string assetPath) where T : class
        {
            yield return LoadAssetAsync<T>(assetPath, null);
        }

        /// <summary>
        /// 预加载某个label的所有资源(回调模式)
        /// </summary>
        /// <param name="lable"></param>
        /// <param name="callback"></param>
        public void PreLoadAssetsByLabelAsync(string lable, Action<LoadAssetsHandle> callback)
        {
            List<string> keysList = GetKeysListByLabel(lable);
            LoadAssetsHandle mainHandle = new LoadAssetsHandle();
            if (keysList.Count == 0)
            {
                mainHandle.isDone = true;
                callback?.Invoke(mainHandle);
            }

            int doneCount = 0;
            List<AsyncOperationHandle> handleList = new List<AsyncOperationHandle>();
            foreach (var key in keysList)
            {
                var handle = LoadAssetAsync<Object>(key, null);
                handleList.Add(handle);
            }

            mainHandle.allCount = handleList.Count;
            uint timerHandle = 0;
            timerHandle = FrameTimer.AddTimer(0, 50, () =>
            {
                doneCount = 0;
                mainHandle.loadedCount = 0;
                foreach (var handle in handleList)
                {
                    if (handle.IsDone)
                    {
                        doneCount++;
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            mainHandle.loadedCount += 1.0f;
                        }
                    }
                    else
                    {
                        mainHandle.loadedCount += handle.PercentComplete;
                    }
                }
                mainHandle.progress = mainHandle.loadedCount / mainHandle.allCount;
                mainHandle.isDone = (doneCount == handleList.Count);
                callback?.Invoke(mainHandle);

                //处理完成删除定时器
                if (doneCount == handleList.Count)
                {
                    foreach (var handle in handleList)
                    {
                        if (handle.IsDone && handle.Status != AsyncOperationStatus.Succeeded)
                        {
                            Debug.LogErrorFormat("[PreLoadAssetsByLabelAsync]加载失败：{0}", handle.DebugName);
                        }
                    }
                    FrameTimer.DelTimer(timerHandle);
                }
            });
        }

        /// <summary>
        /// 预加载某个label的所有资源(协程模式)（有同步加载需求时使用）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <returns></returns>
        public IEnumerator CoPreLoadAssetsByLabel<T>(string label) where T : class
        {
            List<string> keysList = GetKeysListByLabel(label);
            List<AsyncOperationHandle> handleList = new List<AsyncOperationHandle>();
            foreach (var key in keysList)
            {
                var handle = LoadAssetAsync<T>(key, null);
                yield return handle;
                handleList.Add(handle);
            }

            //遍历所有已加载的handle检查是否加载成功
            foreach (var handle in handleList)
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogErrorFormat("预加载批量资源错误，未能成功加载Label名为：{0}中的{1}资源", label, handle.DebugName);
                }
            }
        }

        /// <summary>
        /// 尝试同步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryGetAsset<T>(string assetPath, out T target) where T : class
        {
            target = null;
            bool result = false;
            AsyncOperationHandle handle;
            if (_handleCaches.TryGetValue(assetPath, out handle))
            {
                if (handle.IsDone)
                {
                    target = handle.Result as T;
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 查询一个资源是否已经被预加载（缓存）
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public bool CheckHasCached(string assetPath)
        {
            bool result = false;
            AsyncOperationHandle handle;
            if (_handleCaches.TryGetValue(assetPath, out handle))
            {
                if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion

        #region 资源加载/卸载

        /// <summary>
        /// 异步加载资源(回调/协程都可以调用)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="onComplete">成功加载回调</param>
        /// <param name="autoUnload">是否加载完成后自动卸载</param>
        /// <returns></returns>
        public AsyncOperationHandle LoadAssetAsync<T>(string assetPath, Action<T> onComplete, bool autoUnload = false) where T : class
        {
            //此资源已加载或者正在加载
            if (_handleCaches.ContainsKey(assetPath))
            {
                var handle = this._handleCaches[assetPath];
                if (handle.IsDone)
                {
                    if (onComplete != null)
                    {
                        onComplete(_handleCaches[assetPath].Result as T);
                    }
                }
                else
                {
                    handle.Completed += (result) =>
                    {
                        if (result.Status == AsyncOperationStatus.Succeeded)
                        {
                            var obj = result.Result as T;
                            if (onComplete != null)
                            {
                                onComplete(obj);
                            }
                            if (autoUnload)
                            {
                                UnLoadAsset(assetPath);
                            }
                        }
                        else
                        {
                            onComplete?.Invoke(null);
                            Debug.LogErrorFormat("Load {0} failed", assetPath);
                        }
                    };
                }
                return handle;
            }
            else
            {
                //此资源未加载过
                _loadingAssetCount++;
                _loadedAssetInstanceCountDic.Add(assetPath, 0);

                var handle = Addressables.LoadAssetAsync<T>(assetPath);
                handle.Completed += (result) =>
                {
                    _loadingAssetCount--;
                    if (result.Status == AsyncOperationStatus.Succeeded)
                    {
                        var obj = result.Result as T;
                        if (onComplete != null)
                        {
                            onComplete(obj);
                        }
                        if (autoUnload)
                        {
                            UnLoadAsset(assetPath);
                        }
                    }
                    else
                    {
                        onComplete?.Invoke(null);
                        Debug.LogErrorFormat("Load {0} failed", assetPath);
                    }
                };

                _handleCaches.Add(assetPath, handle);
                return handle;
            }
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        public void UnLoadAsset(string assetPath)
        {
            //判断卸载的是否是一个常驻资源
            if (_residentAssetsHashSet.Contains(assetPath))
            {
                Debug.LogErrorFormat("卸载了一个常驻资源:{0},阻止了此卸载操作,卸载常驻资源是不允许的", assetPath);
                return;
            }

            AsyncOperationHandle handle;
            if (_handleCaches.TryGetValue(assetPath, out handle))
            {
                if (!handle.IsDone)
                {
                    Debug.LogErrorFormat("卸载了一个未加载完毕的资源:{0}", assetPath);
                }
                Addressables.Release(handle);
                _handleCaches.Remove(assetPath);
                _loadedAssetInstanceCountDic.Remove(assetPath);
            }
            else
            {
                Debug.LogErrorFormat("卸载了一个未加载的资源:{0}", assetPath);
            }
        }
        #endregion

        #region 图片加载
        /// <summary>
        /// Sprite 每次获取都会Clone一份,因此需要缓存
        /// </summary>
        public class SpriteAtlas
        {
            public UnityEngine.U2D.SpriteAtlas spriteAtlas;
            private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

            public Sprite Get(string spriteName)
            {
                if (!_spriteCache.TryGetValue(spriteName, out Sprite sprite))
                {
                    sprite = spriteAtlas.GetSprite(spriteName);
                    _spriteCache.Add(spriteName, sprite);
                }
                return sprite;
            }

            public void Release()
            {
                foreach (var sprite in _spriteCache.Values)
                {
                    UnityEngine.Object.Destroy(sprite);
                }
                _spriteCache.Clear();
            }
        }


        private Dictionary<string, SpriteAtlas> _spriteCache = new Dictionary<string, SpriteAtlas>();

        /// <summary>
        /// 异步加载Sprite
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        /// <param name="spriteName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AsyncOperationHandle LoadSpriteAsync(string atlasPath, string spriteName, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(atlasPath) || string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError("Load sprite async failed:" + atlasPath + "/" + spriteName);
                callback.Invoke(null);
                return default;
            }

            if (_spriteCache.TryGetValue(atlasPath, out SpriteAtlas spriteAtlas))
            {
                callback?.Invoke(spriteAtlas.Get(spriteName));
                return default;
            }
            else
            {
                return LoadAssetAsync<UnityEngine.U2D.SpriteAtlas>(atlasPath, (atlas) =>
                {
                    if (atlas == null)
                    {
                        Debug.LogError("load atlas failed path:" + atlasPath);
                        return;
                    }

                    if (!_spriteCache.TryGetValue(atlasPath, out spriteAtlas))
                    {
                        spriteAtlas = new SpriteAtlas { spriteAtlas = atlas };
                        _spriteCache.Add(atlasPath, spriteAtlas);
                    }
                    callback?.Invoke(spriteAtlas.Get(spriteName));
                });
            }
        }
        #endregion

        #region 场景加载
        public SceneInstance MainScene { get; set; }


        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="name">场景名</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="callback">回调函数</param>
        public void LoadSceneAsync(string name, LoadSceneMode loadMode = LoadSceneMode.Single, Action<SceneInstance> callback = null)
        {
            Addressables.LoadSceneAsync(name, loadMode).Completed += (op) =>
            {
                if (loadMode == LoadSceneMode.Additive)
                {
                    _scenenInstanceNameKeyDic[name] = op.Result;
                }
                else
                {
                    MainScene = op.Result;
                }
                callback?.Invoke(op.Result);
            };
        }

        /// <summary>
        /// 异步加载场景：协程模式
        /// </summary>
        /// <param name="name">场景名</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="callback">回调函数</param>
        /// <returns></returns>
        public IEnumerator CoLoadSceneAsync(string name, LoadSceneMode loadMode = LoadSceneMode.Single, Action<SceneInstance> callback = null)
        {
            var handle = Addressables.LoadSceneAsync(name, loadMode);
            handle.Completed += (op) =>
            {
                if (loadMode == LoadSceneMode.Additive)
                {
                    _scenenInstanceNameKeyDic[name] = op.Result;
                }
                else
                {
                    MainScene = op.Result;
                }
                callback?.Invoke(op.Result);
            };
            yield return handle;
        }

        /// <summary>
        /// 通过场景名字(仅适用Additive方式加载的场景)卸载场景
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="canUnload"></param>
        public void UnloadSceneAsyncByName(string name, Action callback, out bool canUnload)
        {
            canUnload = AdditiveSceneLoad(name);
            if (canUnload)
            {
                UnloadSceneAsync(_scenenInstanceNameKeyDic[name], callback);
                _scenenInstanceNameKeyDic.Remove(name);
            }
        }

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneInstance"></param>
        /// <param name="callback"></param>
        public void UnloadSceneAsync(SceneInstance sceneInstance, Action callback)
        {
            Addressables.UnloadSceneAsync(sceneInstance).Completed += (op) =>
            {
                callback?.Invoke();
            };
        }

        /// <summary>
        /// 通过场景名字(仅适用Additive方式加载的场景)异步卸载场景：卸载模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback">回调函数</param>
        /// <returns></returns>
        public IEnumerator CoUnloadSceneAsyncByName(string name, Action callback)
        {
            if (AdditiveSceneLoad(name))
            {
                yield return CoUnloadSceneAsync(_scenenInstanceNameKeyDic[name], callback);
                _scenenInstanceNameKeyDic.Remove(name);
            }
        }


        /// <summary>
        /// 异步卸载场景：协程模式
        /// </summary>
        /// <param name="sceneInstance">场景实例</param>
        /// <param name="callback">回调函数</param>
        /// <returns></returns>
        public IEnumerator CoUnloadSceneAsync(SceneInstance sceneInstance, Action callback)
        {
            var handle = Addressables.UnloadSceneAsync(sceneInstance);
            handle.Completed += (op) =>
            {
                callback?.Invoke();
            };
            yield return handle;
        }
        #endregion

        #region Text读取
        public IEnumerator ReadTextStringAsync(string assetPath, Action<String> callback)
        {
            yield return LoadAssetAsync<TextAsset>(assetPath, (TextAsset asset) =>
            {
                if (assetPath == null)
                {
                    Debug.LogFormat("ReadTextString from path:{0} is null !", assetPath);
                    callback?.Invoke(string.Empty);
                }
                else
                {
                    callback?.Invoke(asset.text);
                }
            });
        }


        public void ReadTextStreamAsync(string assetPath, Action<MemoryStream> callback)
        {
            LoadAssetAsync<TextAsset>(assetPath, (TextAsset asset) =>
            {
                if (asset == null)
                {
                    Debug.LogFormat("ReadTextString from path{0} is null!", assetPath);
                    callback?.Invoke(null);
                }
                else
                {
                    MemoryStream stream = new MemoryStream(asset.bytes);
                    callback?.Invoke(stream);
                }
            });
        }
        #endregion

        #region 设置常驻资源
        //设置常驻资源
        //注意：
        //1、公共包（被2个或者2个其他AB包所依赖的包）底层会自动设置常驻包
        //2、任何情况下不想被卸载的非公告包(如Lua脚本,通用UI资源)需要手动设置常驻包


        /// <summary>
        /// 设置常驻资源：根据资源路径
        /// </summary>
        /// <param name="assetPath"></param>
        public void SetResidentAsset(string assetPath)
        {
            _residentAssetsHashSet.Add(assetPath);
        }

        public void SetResidentAsset(List<string> assetPaths)
        {
            for (int i = 0; i < assetPaths.Count; i++)
            {
                SetResidentAsset(assetPaths[i]);
            }
        }


        /// <summary>
        /// 设置常驻资源：根据label名
        /// </summary>
        /// <param name="label"></param>
        public void SetResidentAssets(string label)
        {
            HashSet<string> residentAssetsSet = GetKeysHashSetByLabel(label);
            if (residentAssetsSet.Count > 0)
            {
                _residentAssetsHashSet.UnionWith(residentAssetsSet);
            }
            else
            {
                Debug.LogErrorFormat("This label <{0}> dont have any assets", label);
            }
        }

        /// <summary>
        /// 设置常驻资源：根据一组label名
        /// </summary>
        /// <param name="label"></param>
        public void SetResidentAssets(List<string> labels)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                SetResidentAssets(labels[i]);
            }
        }
        #endregion
        #region 工具
        /// <summary>
        /// 加载多个Asset所有的Handle数据结构
        /// </summary>
        public class LoadAssetsHandle
        {
            public LoadAssetsHandle()
            {
                isDone = false;
            }

            public float progress { get; set; }

            public float allCount { get; set; }

            public float loadedCount { get; set; }

            private bool _isdone;

            public bool isDone
            {
                get
                {
                    if (allCount == loadedCount)
                    {
                        _isdone = true;
                    }
                    else
                    {
                        _isdone = false;
                    }
                    return _isdone;
                }
                set
                {
                    _isdone = value;
                }
            }

            public void Dispose()
            {

            }
        }

        /// <summary>
        /// 拿到某个label的所有key列表
        /// </summary>
        /// <param name="labelName">Label名字</param>
        /// <returns></returns>
        private List<string> GetKeysListByLabel(string labelName)
        {
            var t = Addressables.ResourceLocators;
            List<string> keys = new List<string>();
            foreach (var locator in t)
            {
                if (!(locator is ResourceLocationMap)) continue;
                ResourceLocationMap locationMap = locator as ResourceLocationMap;
                locationMap.Locate(labelName, typeof(object), out var locationList);
                if (locationList == null)
                {
                    Debug.LogErrorFormat("此 label:{0}没有对应的key", labelName);
                    break;
                }

                Dictionary<string, char> dic = new Dictionary<string, char>();
                foreach (var location in locationList)
                {
                    if (!dic.ContainsKey(location.PrimaryKey))
                        dic.Add(location.PrimaryKey, '1');
                }
                keys = dic.Keys.ToList();
                dic = null;
                break;
            }
            return keys;
        }

        /// <summary>
        /// 拿到某个Labeldel的所有key的HashSet
        /// </summary>
        /// <param name="labelName"></param>
        /// <returns></returns>
        private HashSet<string> GetKeysHashSetByLabel(string labelName)
        {
            var t = Addressables.ResourceLocators;
            HashSet<string> keys = new HashSet<string>();
            foreach (var locator in t)
            {
                locator.Locate(labelName, typeof(object), out var locationList);
                if (locationList == null)
                {
                    continue;
                }

                foreach (var location in locationList)
                {

                    keys.Add(location.PrimaryKey);
                }
            }
            return keys;
        }

        /// <summary>
        /// 输出已载入的资源对象实例状态
        /// </summary>
        public void PrintState()
        {
            foreach (var item in _loadedAssetInstanceCountDic)
            {
                Debug.LogFormat("Asset Key:{0},Count:{1}", item.Key, item.Value);
            }
        }


        /// <summary>
        /// 返回当前正在内存中的资源数量
        /// </summary>
        /// <returns></returns>
        public int GetAssetsNum()
        {
            return _handleCaches.Count;
        }


        /// <summary>
        /// 判断通过Additive加载的场景是否加载完
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AdditiveSceneLoad(string name)
        {
            return _scenenInstanceNameKeyDic.ContainsKey(name);
        }
        #endregion
    }
}
