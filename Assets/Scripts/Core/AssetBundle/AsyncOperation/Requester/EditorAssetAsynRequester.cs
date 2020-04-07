using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFrame.AssetBundles
{
    public class EditorAssetAsynRequester : ResourceAsyncOperation
    {
        protected bool isOver = true;

        protected Object targetAsset = null;


        /// <summary>
        /// 模拟加载延时范围
        /// </summary>
        private float minWaitTime = 0.2f;
        private float maxWaitTime = 0.5f;

        private float totalWaitTime = 0.2f;
        private float remainWaitTime = 0.2f;

        public EditorAssetAsynRequester()
        {

        }

        public void Init(string assetName,Type assetType,bool noCache = false)
        {
            this.assetName = assetName;
            this.assetType = assetType;
            this.noCache = noCache;
            targetAsset = null;
            isOver = false;
        }




        public bool noCache
        {
            get;
            protected set;
        }

        public Type assetType
        {
            get;
            protected set;
        }

        public string assetName
        {
            get;
            protected set;
        }

        public Object asset
        {
            get;
            protected set;
        }

        public string error
        {
            get
            {
                return string.Empty;
            }
        }

        public override bool IsDone()
        {
            return isOver;
        }

        public void Start()
        {
            string path = AssetBundlePath.PackagePathToAssetsPath(this.assetName);
#if UNITY_EDITOR
            //这里一开始就把资源请求到，先缓存下来，单当模拟延时结束才赋给“asset”
            this.targetAsset = AssetDatabase.LoadAssetAtPath(path.ToLower(),assetType);
#endif
            if(targetAsset ==null)
            {
                AssetBundleManager.Instance.AddToLoadFaildCatch(path);
                Debug.LogError(string.Format("EditorAssetAsynRequester Error : No Asset At Path{0}!",path));
                isOver = true;
                return;
            }

            this.totalWaitTime = this.GetRandomWaitTime();
            this.remainWaitTime = this.totalWaitTime;
        }


        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }
            float progressValue = this.totalWaitTime != 0 ? (this.totalWaitTime - this.remainWaitTime) / this.totalWaitTime : 0.0f;
            return progressValue;
        }

        public override void Update(float deltaTime = 0)
        {
            if (isDone)
            {
                return;
            }

            if(this.remainWaitTime>0)
            {
                this.remainWaitTime -= deltaTime;
            }
            else
            {
                this.remainWaitTime = 0;
                asset = this.targetAsset;
                isOver = true;
            }
        }


        private float GetRandomWaitTime()
        {
            return UnityEngine.Random.Range(minWaitTime,maxWaitTime);
        }
        public override void Dispose()
        {
            isOver = true;
            totalWaitTime = 1;
            remainWaitTime = 1;
            assetName = null;
            targetAsset = null;
            assetType = null;
            isOver = true;
            if (base.holder != null)
            {
                base.Release();
            }
        }
    }
}
