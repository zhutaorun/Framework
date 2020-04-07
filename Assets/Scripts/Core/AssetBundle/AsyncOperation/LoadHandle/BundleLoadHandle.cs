using UnityEngine;
using System.Collections;

/// <summary>
/// 功能：assetbundle 在bundle模式下的同步加载器句柄
/// </summary>
namespace GameFrame.AssetBundles
{
    public class BundleLoadHandle : BaseAssetBundleAsyncLoadHandle
    {
        public BundleLoadHandle(string assetbundleName,AssetBundle assetbundle)
        {
            this.bundleName = assetbundleName;
            this.assetbundle = assetbundle;
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
