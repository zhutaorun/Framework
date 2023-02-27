using GameFrame.Pool;
using GameFrame.Skill.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    //Entity Transform
    public class ComponentTransform : PoolClass
    {
        public GameObject RootGameObj;

        public Transform RootGameObjTransform;

        public Dictionary<string, Transform> Dummys = new Dictionary<string, Transform>();//挂点

        public bool SelfControl;

        public Transform GetCacheDummy(string attach)
        {
            Transform dummy = null;
            if(Dummys.TryGetValue(attach,out dummy))
            {
                return dummy;
            }
            if(RootGameObjTransform != null)
            {
                dummy = UtilityCommon.FindChildByName(attach,RootGameObjTransform);
                if(dummy != null)
                {
                    Dummys.Add(attach, dummy);
                    return dummy;
                }
            }
            return RootGameObjTransform;
        }

        public override void OnUse()
        {
            base.OnUse();
        }

        public override void OnRelease()
        {
            base.OnRelease();
            if (SelfControl)
                GameObject.Destroy(RootGameObj);
            RootGameObj = null;
            RootGameObjTransform = null;
            Dummys.Clear();
            SelfControl = false;
        }
    }
}
