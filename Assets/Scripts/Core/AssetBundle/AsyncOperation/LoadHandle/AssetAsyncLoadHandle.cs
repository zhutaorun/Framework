using System;
using Object = UnityEngine.Object;

/// <summary>
/// 功能：Asset异步加载器，自动追踪依赖的ab加载进度
/// 说明：
/// 1.一定要所有ab都加载完毕以后再加载asset，所以这里分成两个加载步骤
/// 2.加载器AssetBundleManager只负责调度，创建，不负责回收，逻辑层代码使用完一定要记得回收，否则会产生GC
/// </summary>

namespace GameFrame.AssetBundles
{
    public class AssetAsyncLoadHandle :BaseAssetAsyncLoadHandle
    {
        //isOver这个状态默认为True,为了控制update的执行，为true时update不执行
        public bool isOver = true;
        private bool bundleIsOver = false;

        private BaseAssetBundleAsyncLoadHandle bundleLoader { get; set; }

        public AssetAsyncLoadHandle()
        {

        }

        public void Init(string assetName, string assetbundleName, BaseAssetBundleAsyncLoadHandle loader,
            Action<BaseAssetAsyncLoadHandle> overCallback = null,bool autoDispose = false,bool catchAsset = true)
        {
            this.catchAsset = catchAsset;
            this.assetName = assetName;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;

            asset = null;
            bundleName = assetbundleName;
            isOver = false;
            bundleIsOver = false;
            bundleLoader = loader;
        }

        public void Init(string assetName, string bundleName, Object asset,
            Action<BaseAssetAsyncLoadHandle> overCallback = null, bool autoDispose = false)
        {
            this.catchAsset = true;
            this.assetName = assetName;
            this.asset = asset;
            this.overCallback = overCallback;
            this.autoDispose = autoDispose;
            this.bundleName = bundleName;

            bundleLoader = null;
            isOver = true;
            bundleIsOver = true;

            if (this.overCallback != null)
            {
                this.overCallback.Invoke(this);
            }
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
            if(isDone)
            {
                return 1.0f;
            }

            var creater = AssetBundleManager.Instance.GetAssetRequester(assetName);
            //这里粗略估计加载Bundle时间占0.1
            //一般情况，Bundle已经加载过
            if(creater == null)
            {
                return 0.1f * bundleLoader.progress;
            }
            else
            {
                return 0.1f + 0.9f * creater.progress;
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
                AssetBundleManager.Instance.CreateAssetRequester(assetName, bundleName);
                bundleLoader.Dispose();
            }

            if (!AssetBundleManager.Instance.IsAssetLoaded(assetName))
            {
                return;
            }
            else
            {
                asset = AssetBundleManager.Instance.GetAssetCache(assetName);
                //从缓存中移除
                if(!catchAsset)
                {
                    AssetBundleManager.Instance.RemoveAssetCache(assetName);
                }
                if(!isCanel) //简单的处理是先不回调，否则要处理bundle加载
                {
                    if(overCallback !=null)
                    {
                        overCallback.Invoke(this);
                    }
                }
                isOver = true;
            }
        }


        public override void Dispose()
        {
            isOver = true;
            bundleIsOver = false;
            isCanel = false;
            assetName = null;
            bundleName = null;
            asset = null;
            overCallback = null;
            bundleLoader = null;
            autoDispose = false;
            catchAsset = false;
            if (base.holder != null)
            {
                base.Release();
            }
        }
    }
}
