using EffectConfig;
using GameFrame.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    public class ComponentEffect : PoolClass
    {
        // 全局额外的特效缩放值
        public static float EffectScale = 1;

        public effect_data EffectCfg;

        public Transform EffectObjTrans;

        public float EffectElapse;//特效经过多久

        public GameObject EffectObj;

        public int AttachEntity;

        public Vector3 EffectOffset;

        public Quaternion DirOffset;

        public uint EffectUID;

        public bool SkillOverRemove;//技能结束删除，暂时写在这里，如果条件多了就读配置

        public bool SkillActionOverRemove;

        public bool Valid;

        public bool Inited;

        //加载结束，但一定加载到了资源，控制只加载一次
        public bool LoadOver;

        public Vector3 StartLocalPos;

        public bool SkillAdjustHeight;

        public override void OnRelease()
        {
            base.OnRelease();
            Valid = false;
            EffectUID = 0;
            EffectOffset = default(Vector3);
            EffectObj = null;
            EffectElapse = 0;
            EffectObjTrans = null;
            EffectCfg = null;
            Inited = false;
            LoadOver = false;
            AttachEntity = 0;
            SkillActionOverRemove = false;
            StartLocalPos = default(Vector3);
            SkillAdjustHeight = false;
            SkillActionOverRemove = false;
        }
    }
}
