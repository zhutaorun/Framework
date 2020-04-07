using ClassPool;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame.AssetBundles
{
    public abstract class ResourceAsyncOperation : PoolClass, IEnumerator, IDisposable
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool isDone
        {
            get
            {
                return IsDone();
            }
        }

        public float progress
        {
            get
            {
                return Progress();
            }
        }

        abstract public void Update(float deltaTime = 0);

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {

        }
        

        abstract public bool IsDone();

        abstract public float Progress();

        public virtual void Dispose()
        {
           
        }
    }


    abstract public class BaseAssetBundleAsyncLoadHandle: ResourceAsyncOperation
    {
        public string bundleName
        {
            get;
            protected set;
        }

        public int bundleIndex
        {
            get;
            protected set;
        }

        public AssetBundle assetbundle
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            bundleName = null;
            assetbundle = null;
        }
    }

    abstract public class BaseAssetAsyncLoadHandle:ResourceAsyncOperation
    {
        public bool autoDispose = false;
        public bool isCanel = false;

        public Action<BaseAssetAsyncLoadHandle> overCallback { get; set; }

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

        public bool catchAsset
        {
            get;
            protected set;
        }

        public override void Dispose()
        {
            autoDispose = false;
            assetName = null;
            asset = null;
            isCanel = false;
        }
    }
}
