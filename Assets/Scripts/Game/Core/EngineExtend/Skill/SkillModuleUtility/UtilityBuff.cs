using BuffConfig;
using GameFrame.Skill.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Utility
{
    public static class UtilityBuff
    {
        /// <summary>
        /// buff结束,位移这种特殊结束方式需求内部结束
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="buffId"></param>
        public static void BuffOver(int entityId, int buffId)
        {
            ComponentBuff buff = SkillController.Component.GetComponent<ComponentBuff>(entityId);
            if(buff.BuffConfig!= null)
            {
                for (int value = 0; value < buff.BuffConfig.BuffStartEffects.Count; value++)
                    UtilityEffect.PlayEffect(entityId, buff.AttachEntity, (int)buff.BuffConfig.BuffEndEffects[value]);
            }

            SkillController.ID.ReleaseModuleId(entityId);
            SkillController.Component.RemoveEntityComponent(entityId);
        }


        public static bool CreateBuff(int entityId,int attachEntityId,GameObject attachEntity,int buffId,SkillTriggerParam triggerParam = default(SkillTriggerParam))
        {
            buff_data config = SkillController.SkillCfg.Get<buff_data>(buffId);
            if(config == null)
            {
                Debug.LogWarningFormat("[CreateBuff] Not found buff_data:id = {0}",buffId);
                return false;
            }
            ComponentBuff buff = SkillController.Component.GetComponent<ComponentBuff>(entityId);

            buff.AddBuff(config,attachEntityId,triggerParam);
            UtilitySkill.InitAnimationComponents(attachEntity,attachEntityId);
            return true;
        }
    }

}