using UnityEngine;
using System;

/// <summary>
/// 功能：空Asset加载器，只是用来传回句柄，处理回调
/// </summary>
namespace GameFrame.AssetBundles
{
    public class NullAssetAsyncLoadHandle:BaseAssetAsyncLoadHandle
    {
        public NullAssetAsyncLoadHandle(string assetbundleName,Action<BaseAssetAsyncLoadHandle> overCallback = null)
        {
            base.overCallback = overCallback;

            if(overCallback !=null)
            {
                overCallback.Invoke(this);
            }
        }

        public override bool IsDone()
        {
            return true;
        }

        public override float Progress()
        {
            return 1.0f;
        }

        public override void Update(float deltaTime = 0)
        {
        }

        public override void Dispose()
        {

        }
    }
}
