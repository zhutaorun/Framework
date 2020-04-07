using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 功能：Asset List 异步加载器
/// 说明：
/// 1.加载器AssetBundleManager只负责调度，创建，不负责回收，逻辑层代码使用完一定要记得回收，否则会产生GC
/// </summary>

namespace GameFrame.AssetBundles
{
    public class AssetListAsyncLoadHandle:BaseAssetAsyncLoadHandle
    {
        public bool isOver = true;
        protected List<BaseAssetAsyncLoadHandle> waitingList = new List<BaseAssetAsyncLoadHandle>();
        protected int waitingCount = 0;

        public AssetListAsyncLoadHandle()
        {
        }

        public void Init(List<BaseAssetAsyncLoadHandle> assetLoaders,Action<BaseAssetAsyncLoadHandle> overCallback = null)
        {
            isOver = false;
            waitingList = assetLoaders;
            this.overCallback = overCallback;
            for(int i=0;i<waitingList.Count;i++)
            {
                if (waitingList[i] == null || waitingList[i].isDone)
                    waitingList.Remove(waitingList[i]);
            }
            waitingCount = waitingList.Count;
        }

        public void Init(Action<BaseAssetAsyncLoadHandle> overCallback = null)
        {
            this.overCallback = overCallback;
            isOver = true;
            if(this.overCallback != null)
            {
                this.overCallback.Invoke(this);
            }
        }


        public int Sequence
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

            float progressSlice = 1.0f / waitingCount;
            float progressValue = (waitingCount - waitingList.Count) * progressSlice;
            float subProgress = 0.0f;
            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                subProgress += (waitingList[i] != null ? waitingList[i].progress :0.0f) * progressSlice;
            }

            progressValue = progressValue + subProgress;
            return progressValue;
        }

        public override void Update(float deltaTime = 0)
        {
            if(isDone)
            {
                return;
            }

            for(int i=waitingCount-1;i>=0;i--)
            {
                if(waitingList[i] == null)
                {
                    waitingList.RemoveAt(i);
                }
            }

            for(int i=waitingList.Count -1;i>=0;i--)
            {
                if(waitingList[i].isDone)
                {
                    waitingList.Remove(waitingList[i]);
                }
            }

            if(waitingList.Count ==0)
            {
                isOver = true;
                if(overCallback != null)
                {
                    overCallback.Invoke(this);
                }
            }
        }

        public override void Dispose()
        {
            isOver = true;
            for(int i=0;i<this.waitingList.Count;i++)
            {
                if (waitingList[i] != null)
                    waitingList[i].Dispose();
            }

            waitingList.Clear();
            waitingCount = 0;
            if(base.holder != null)
            {
                base.Release();
            }
        }
    }
}
