using GameFrame.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    /// <summary>
    /// Entity旋转相关的数据
    /// </summary>
    public class ComponentRotate : PoolClass
    {
        public Quaternion LookDirQuaternion;
        public Vector3 Dir;

        public override void OnRelease()
        {
            base.OnRelease();

            LookDirQuaternion = default(Quaternion);
            Dir = Vector3.zero;
        }
    }
}
