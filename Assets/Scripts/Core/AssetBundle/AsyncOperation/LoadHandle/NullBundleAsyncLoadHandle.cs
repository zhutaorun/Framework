using UnityEngine;
using System.Collections;

/// <summary>
/// 功能：assetbundle 在simulate模式下的Asset模拟异步加载器
/// </summary>
namespace GameFrame.AssetBundles
{
    public class NullBundleAsyncLoadHandle:BaseAssetBundleAsyncLoadHandle
    {
        public NullBundleAsyncLoadHandle(string assetbundleName)
        {
            this.bundleName = assetbundleName;
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
