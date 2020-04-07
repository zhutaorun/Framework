using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// 功能：assetbundle 在simulate模式下的Asset模拟异步加载器句柄
/// </summary>
namespace GameFrame.AssetBundles
{ 
    public class EditorAssetAsyncLoadHandle:BaseAssetAsyncLoadHandle
    {
        protected bool isOver = true;

        public EditorAssetAsyncLoadHandle()
        {
        }

        public void Init(string assetName,Object asset,
            Action<BaseAssetAsyncLoadHandle> overCallback = null,bool autoDispose = false)
        {
            isOver = true;
            this.catchAsset = true;
            this.assetName = assetName;
            this.asset = asset;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;
            if(this.overCallback != null)
            {
                this.overCallback.Invoke(this);
            }
        }

        public void Init(string assetName, Type assetType,
            Action<BaseAssetAsyncLoadHandle> overCallback = null, bool autoDispose = false,bool catchAsset = true)
        {
            isOver = false;
            asset = null;
            this.catchAsset = catchAsset;
            this.assetName = assetName;
            this.assetType = assetType;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;
        }

        public Type assetType
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
#if UNITY_EDITOR
            if(isDone)
#endif
            {
                return 1.0f;
            }
#if UNITY_EDITOR
            var creater = AssetBundleManager.Instance.GetEditorAssetRequester(assetName);
            if(creater == null)
            {
                return creater.progress;
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
            if(isDone || assetName== null)
            {
                return;
            }

            isOver = AssetBundleManager.Instance.IsAssetLoaded(assetName);
            if(!isOver)
            {
                return;
            }
            else
            {
                asset = AssetBundleManager.Instance.GetAssetCache(assetName);

                //从缓存中移除
                if (!catchAsset)
                {
                    AssetBundleManager.Instance.RemoveAssetCache(assetName);
                }
                if(!isCanel)
                {
                    if(overCallback != null)
                    {
                        overCallback(this);
                    }
                }
            }
#endif
        }


        public override void Dispose()
        {
            isCanel = false;
            isOver = false;
            overCallback = null;
            assetName = null;
            asset = null;
            autoDispose = false;
            catchAsset = false;
            if(base.holder != null)
            {
                base.Release();
            }
        }
    }
}
