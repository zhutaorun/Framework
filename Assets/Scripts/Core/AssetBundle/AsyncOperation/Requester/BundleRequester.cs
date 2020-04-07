using UnityEngine;
namespace GameFrame.AssetBundles
{ 
    public class BundleRequester : ResourceAsyncOperation
    {
        protected bool isOver = true;

        protected AssetBundleCreateRequest request = null;


        public BundleRequester()
        {

        }

        public void Init(string name, string url, bool noCache = false)
        {
            this.url = url;
            this.noCache = noCache;
            this.assetbundleName = name;
            request = null;
            isOver = false;
        }


        public int Sequence
        {
            get;
            protected set;
        }

        public bool noCache
        {
            get;
            protected set;
        }

        public string assetbundleName
        {
            get;
            protected set;
        }

        public string url
        {
            get;
            protected set;
        }

        public AssetBundle assetBundle
        {
            get
            {
                return request == null ? null : request.assetBundle;
            }
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
            request = AssetBundle.LoadFromFileAsync(url);
            if (request == null)
            {
                Debug.LogError("New LoadFromFileAsync failed !!!");
                isOver = true;
            }
        }


        public override float Progress()
        {
            if(isDone)
            {
                return 1.0f;
            }
            return request != null ? request.progress : 0f;
        }

        public override void Update(float deltaTime = 0)
        {
            if(isDone)
            {
                return;
            }
            isOver = request != null && (request.isDone);
            if(!isOver)
            {
                return;
            }
        }

        public override void Dispose()
        {
            isOver = true;
            if(request!=null)
            {
                request = null;
            }
            if(base.holder!=null)
            {
                base.Release();
            }
        }
    }
}
