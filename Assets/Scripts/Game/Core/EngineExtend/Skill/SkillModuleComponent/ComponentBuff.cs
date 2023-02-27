using BuffConfig;
using GameFrame.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    /// <summary>
    /// 管理buff特效资源
    /// </summary>
    public class ComponentBuff : PoolClass
    {
        public int AttachEntity;
        public bool Inited = false;
        public bool Overed = false;
        public buff_data BuffConfig;

        public int BuffLoopEffectCount = 0;

        public bool NeedCheckBuffLoopEffect;

        public bool LogicNtfPlayLoopEffect;

        public SkillTriggerParam FromSkillParam;

        public int AttackerEntityId;

        public void AddBuff(buff_data buff,int attachEntity,SkillTriggerParam tiggerSkillParam)
        {
            BuffConfig = buff;
            AttachEntity = attachEntity;
            FromSkillParam = tiggerSkillParam;
            Inited = false;
            Overed = false;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            Inited = false;
            Overed = true;
            AttachEntity = 0;
            BuffLoopEffectCount = 0;
            NeedCheckBuffLoopEffect = false;
            LogicNtfPlayLoopEffect = false;
            FromSkillParam = default(SkillTriggerParam);
            AttackerEntityId = 0;
        }
    }
}
