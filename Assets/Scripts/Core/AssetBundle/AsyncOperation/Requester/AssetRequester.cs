using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GameFrame.AssetBundles
{
    public class AssetRequester:ResourceAsyncOperation
    {
        protected bool isOver = true;

        protected AssetBundleRequest request = null;


        public AssetRequester()
        {

        }

        public void Init(string bundleName, string assetName, bool noCache = false)
        {
            this.bundleName = bundleName;
            this.assetName = assetName;
            this.noCache = noCache;
            request = null;
            isOver = false;
        }


       

        public bool noCache
        {
            get;
            protected set;
        }

        public string bundleName
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
            get
            {
                return request == null? null:request.asset;
            }
        }

        public override bool IsDone()
        {
            return isOver;
        }

        public void Start()
        {
            int bundleIndex = AssetBundleManager.Instance.CurBundleManifest.GetAssetBundleIndex(bundleName);
            AssetBundle curAssetBundle = AssetBundleManager.Instance.GetAssetBundleCache(bundleIndex);
            if(curAssetBundle ==null)
            {
                Debug.LogError("AssetBundle:" +bundleName +"is Null!!!");
                isOver = true;
                return;
            }
            var assetPath = AssetBundlePath.PackagePathToAssetsPath(assetName);
            request = curAssetBundle.LoadAssetAsync(assetPath);
            if (request == null)
            {
                Debug.LogError("New LoadAssetAsync failed !!!");
                isOver = true;
            }
        }


        public override float Progress()
        {
            if (isDone)
            {
                return 1.0f;
            }
            return request != null ? request.progress : 0f;
        }

        public override void Update(float deltaTime = 0)
        {
            if (isDone)
            {
                return;
            }
            isOver = request != null && (request.isDone);
            if (!isOver)
            {
                return;
            }
        }

        public override void Dispose()
        {
            isOver = true;
            bundleName = null;
            assetName = null;
            request = null;
            if (base.holder != null)
            {
                base.Release();
            }
        }
    }
}
