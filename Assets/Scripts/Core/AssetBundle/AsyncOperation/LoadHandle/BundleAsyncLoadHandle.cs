using System.Collections.Generic;

namespace GameFrame.AssetBundles
{
    public class BundleAsyncLoadHandle : BaseAssetBundleAsyncLoadHandle
    {

        protected List<string> waitingList = new List<string>();
        protected int waitingCount = 0;
        protected bool isOver = true;

        public BundleAsyncLoadHandle()
        {
            Sequence = sequence;
        }

        private int sequence = 0;

        public int Sequence
        {
            get;
            protected set;
        }

        public void Init(string bundleName,int bundleIndex,string[] dependances)
        {
            this.bundleName = bundleName;
            this.bundleIndex = bundleIndex;
            isOver = false;
            waitingList.Clear();
            //说明：只添加没有被加载过的
            assetbundle = AssetBundleManager.Instance.GetAssetBundleCache(bundleIndex);
            if(assetbundle)
            {
                waitingList.Add(base.bundleName);
            }
            if(dependances !=null && dependances.Length >0)
            {
                for(int i=0;i<dependances.Length;i++)
                {
                    var ab = dependances[i];
                    int abIndex = AssetBundleManager.Instance.CurBundleManifest.GetAssetBundleIndex(ab);
                    if (!AssetBundleManager.Instance.IsAssetBundleLoaded(abIndex))
                    {
                        waitingList.Add(dependances[i]);
                    }
                }
            }
            waitingCount = waitingList.Count;
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

            float progressSlice = 1.0f / waitingCount;
            float progressValue = (waitingCount - waitingList.Count) * progressSlice;
            for(int i = waitingList.Count-1;i>=0;i--)
            {
                var cur = waitingList[i];
                var creater = AssetBundleManager.Instance.GetBundleRequester(cur);
                progressValue += (creater != null ? creater.progress : 1.0f) * progressSlice;
            }

            return progressValue;
        }


        public override void Update(float deltaTime = 0)
        {
            if(isDone || bundleName == null)
            {
                return;
            }

            for(int i=waitingList.Count-1;i>=0;i--)
            {
                int abIndex = AssetBundleManager.Instance.CurBundleManifest.GetAssetBundleIndex(waitingList[i]);
                if(AssetBundleManager.Instance.IsAssetBundleLoaded(abIndex))
                {
                    var curFinished = waitingList[i];
                    if(curFinished == bundleName)
                    {
                        assetbundle = AssetBundleManager.Instance.GetAssetBundleCache(bundleIndex);
                    }
                    waitingList.RemoveAt(i);
                }
            }
            isOver = waitingList.Count == 0;
        }

        public override void Dispose()
        {
            isOver = true;
            waitingList.Clear();
            waitingCount = 0;
            bundleName = null;
            assetbundle = null;
            if (base.holder != null)
            {
                base.Release();
            }
        }
    }
}
