using UnityEngine;
using System.Collections;

/// <summary>
/// 功能：assetbundle 在bundle模式下的同步加载器
/// </summary>
namespace GameFrame.AssetBundles
{
    public class AssetLoadHandle:BaseAssetAsyncLoadHandle
    {

        public AssetLoadHandle(Object obj)
        {
            asset = obj;
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
