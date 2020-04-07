using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// 功能：Scene 异步加载器,自动追踪依赖的asset加载进度
/// </summary>
namespace GameFrame.AssetBundles
{
    public class SceneAsyncLoadHandle :BaseAssetAsyncLoadHandle
    {
        public bool isOver = true;
        private bool bundleIsOver = false;

        private BaseAssetBundleAsyncLoadHandle bundleLoader { get; set; }


        public string sceneName;
        public LoadSceneMode loadSceneMode;
        public AsyncOperation loadOperation;

        public SceneAsyncLoadHandle()
        {
        }

        public void Init(string assetName,string bundleName,string sceneName,LoadSceneMode mode,BaseAssetBundleAsyncLoadHandle bundleLoader,
            Action<BaseAssetAsyncLoadHandle> overCallback = null,bool autoDispose= false)
        {
            this.assetName = assetName;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;
            this.loadSceneMode = mode;
            this.sceneName = sceneName;

            asset = null;
            isOver = false;
            bundleIsOver = false;
            this.bundleLoader = bundleLoader;
        }

        public void StartLoad()
        {
            loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName,loadSceneMode);
        }


        public string bundleName
        {
            get;
            protected set;
        }

        public override bool IsDone()
        {
            return isOver;
        }

        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }

            if(loadOperation == null)
            {
                return 0.1f * bundleLoader.progress;
            }
            else
            {
                return 0.1f + 0.9f * loadOperation.progress;
            }
        }

        public override void Update(float deltaTime = 0)
        {
            if(isDone || assetName == null)
            {
                return;
            }
            //Bundle加载结束开始加载Asset
            if(!bundleIsOver && bundleLoader.isDone)
            {
                bundleIsOver = true;
                StartLoad();
                bundleLoader.Dispose();
            }

            if(loadOperation == null || !loadOperation.isDone)
            {
                return;
            }
            else
            {
                if(overCallback != null)
                {
                    overCallback.Invoke(this);
                }
                isOver = true;
            }
        }

        public override void Dispose()
        {
            isOver = true;
            bundleIsOver = false;

            assetName = null;
            bundleName = null;
            asset = null;
            overCallback = null;
            bundleLoader = null;
            autoDispose = false;
            if(base.holder != null)
            {
                base.Release();
            }
        }

    }
}
