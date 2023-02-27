namespace DSFramework.Function.Resource
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 资源使用及管理接口定义
    /// </summary>
    public interface IResourceManager 
    {
        #region 初始化/清理
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        bool IsInitialized();

        /// <summary>
        /// 初始化(回调模式)
        /// </summary>
        /// <param name="completedCallback"></param>
        void InitializeAsync(Action completedCallback);

        /// <summary>
        /// 清空所有(协程模式)
        /// </summary>
        /// <returns></returns>
        IEnumerator CoCleanup();


        /// <summary>
        /// 清空所有（回调模式）
        /// </summary>
        void Cleanup();
        #endregion

        #region 实例化和回收对象
        /// <summary>
        /// 实例化对象（回调模式）
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callBack">回调函数</param>
        /// <param name="active">是否激活</param>
        /// <param name="autoUnload"></param>
        void InstantiateAsync(string assetPath, Action<GameObject> callBack, bool active = true, bool autoUnload = false);


        /// <summary>
        /// 实例化对象(协程+回调模式，用于模拟同步)
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="active">是否激活</param>
        /// <returns></returns>
        IEnumerator CoInstantiateAsync(string assetPath,Action<GameObject> callback,bool active = true);


        /// <summary>
        /// 回收某个对象实例化入对象池
        /// </summary>
        /// <param name="instanceObject"></param>
        /// <param name="forceDestroy"></param>
        void Recycle(GameObject instanceObject, bool forceDestroy = false);
        #endregion

        #region 预加载/缓存

        /// <summary>
        /// 预加载一个资源(回调模式)
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调函数</param>
        void PreLoadAssetAsync<T>(string assetPath,Action<T> callback = null) where T: class;

        /// <summary>
        /// 预加载一个资源(协程模式)(有同步加载需求时候使用)
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        IEnumerator CoPreLoadAsset<T>(string assetPath) where T : class;


        /// <summary>
        /// 预加载某个label的所有资源(回调模式)
        /// </summary>
        /// <param name="label">label名</param>
        /// <param name="callback">回调函数</param>
        void PreLoadAssetsByLabelAsync(string label,Action<ResourceManager.LoadAssetsHandle> callback);

        /// <summary>
        /// 预加载某个label的所有资源(协程模式)(有同步加载需求时使用)
        /// </summary>
        /// <param name="T">资源类型</param>
        /// <param name="label">label名</param>
        /// <returns></returns>
        IEnumerator CoPreLoadAssetsByLabel<T>(string label) where T : class;


        /// <summary>
        /// 尝试同步加载一个已加载的资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="target">资源</param>
        /// <returns></returns>
        bool TryGetAsset<T>(string assetPath, out T target) where T : class;
        #endregion

        #region 资源加载/卸载
        /// <summary>
        /// 异步加载资源(回调/协程都可以调用)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="onComplete">成功加载回调</param>
        /// <param name="autoUnload">是否加载完成后自动卸载</param>
        /// <returns>加载句柄</returns>
        AsyncOperationHandle LoadAssetAsync<T>(string assetPath, Action<T> onComplete,bool autoUnload = false) where T: class;

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        void UnLoadAsset(string assetPath);
        #endregion

        #region 图片加载
        /// <summary>
        /// 异步加载Sprite
        /// </summary>
        /// <param name="assetPath">图集路径</param>
        /// <param name="spriteName">Sprite名</param>
        /// <param name="callback">回调函数</param>
        /// <returns></returns>
        AsyncOperationHandle LoadSpriteAsync(string assetPath, string spriteName, Action<Sprite> callback);
        #endregion

        #region 场景加载
        /// <summary>
        /// 主场景
        /// </summary>
        SceneInstance MainScene { get; }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="nane">场景名</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="callback">回调函数</param>
        void LoadSceneAsync(string nane, LoadSceneMode loadMode= LoadSceneMode.Single, Action<SceneInstance> callback = null);

        /// <summary>
        /// 异步加载场景：协程模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadMode"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IEnumerator CoLoadSceneAsync(string name, LoadSceneMode loadMode = LoadSceneMode.Single, Action<SceneInstance> callback = null);


        /// <summary>
        /// 通过场景名字（仅适用Additive方式加载的场景）卸载场景
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="canUnload"></param>
        void UnloadSceneAsyncByName(string name,Action callback,out bool canUnload);

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneInstance">场景实例</param>
        /// <param name="callback">回调函数</param>
        void UnloadSceneAsync(SceneInstance sceneInstance,Action callback);

        /// <summary>
        /// 通过场景名字(仅适用Additive方式加载的场景)异步卸载场景：协程模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IEnumerator CoUnloadSceneAsyncByName(string name,Action callback);

        /// <summary>
        /// 异步卸载场景:协程模式
        /// </summary>
        /// <param name="sceneInstance"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IEnumerator CoUnloadSceneAsync(SceneInstance sceneInstance, Action callback);
        #endregion

        #region Text读取
        IEnumerator ReadTextStringAsync(string assetPath, Action<String> callback);

        void ReadTextStreamAsync(string assetPath, Action<System.IO.MemoryStream> callback);
        #endregion

        #region 工具

        /// <summary>
        /// 返回当前正在内存中的资源数量
        /// </summary>
        /// <returns></returns>
        int GetAssetsNum();
        #endregion
    }
}