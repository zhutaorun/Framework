using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityEngine;

/// <summary>
/// 功能：assetbundle 在simulate模式下的场景异步加载器句柄
/// </summary>
namespace GameFrame.AssetBundles
{
    public class EditorSceneAsyncLoadHandle:BaseAssetAsyncLoadHandle
    {
        protected bool isOver = true;

        public string sceneName;
        public LoadSceneMode loadSceneMode;
        public AsyncOperation loadOperation;

        public EditorSceneAsyncLoadHandle()
        {
        }


        public void Init(string assetName,string sceneName,LoadSceneMode mode,
            Action<BaseAssetAsyncLoadHandle>  overCallback = null,bool autoDispose= false)
        {
            isOver = false;
            this.assetName = assetName;
            this.loadSceneMode = mode;
            this.sceneName = sceneName;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;
        }

        public void StartLoad()
        {
            loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName,loadSceneMode);
        }


        public override bool IsDone()
        {
            return isOver;
        }

        public override float Progress()
        {
#if UNITY_EDITOR
            if (isDone)
#endif
            {
                return 1.0f;
            }
#if UNITY_EDITOR
            if(loadOperation == null)
            {
                return loadOperation.progress;
            }
            else
            {
                return 0.0f;
            }
#endif
        }

        public override void Update(float deltaTime = 0)
        {
#if UNITY_EDITOR
            if(isDone || assetName == null)
            {
                return;
            }

            isOver = loadOperation.isDone;
            if (!isOver)
            {
                return;
            }
            else
            {
                if (overCallback != null)
                {
                    overCallback(this);
                }
            }
#endif
        }

        public override void Dispose()
        {
            isOver = false;
            overCallback = null;
            assetName = null;
            asset = null;
            autoDispose = false;
            if(base.holder != null)
            {
                base.Release();
            }
        }
    }
}
