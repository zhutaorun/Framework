using UnityEngine;
using Object= UnityEngine.Object;

/// <summary>
/// 功能：asset 在Editor模式下的同步加载器句柄
/// </summary>
namespace GameFrame.AssetBundles
{
    public class EditorAssetLoadHandle:BaseAssetAsyncLoadHandle
    {
        public EditorAssetLoadHandle(Object obj)
        {
            asset = obj;
        }

        public override void Update(float deltaTime = 0)
        {
        }

        public override bool IsDone()
        {
            return true;
        }

        public override float Progress()
        {
            return 1.0f;
        }
    }
}
