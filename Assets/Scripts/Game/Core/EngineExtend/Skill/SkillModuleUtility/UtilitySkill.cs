using GameFrame.Skill.Component;
using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Utility
{
    public static class UtilitySkill
    {
        /// <summary>
        /// buff结束，位移这种特殊结束方式需要内部结束
        /// </summary>
        /// <param name="rangeType"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static float GetConfigRangeByType(ESkillRangeType rangeType,skillnew_data config)
        {
            if(config == null)
            {
                Debug.LogError("GetConfigRangeType config null!");
                return 0;
            }
            float ret = config.SkillRange / 100.0f;
            switch(rangeType)
            {
                case ESkillRangeType.Range:
                    ret = config.SkillRange / 100.0f;
                    break;
                case ESkillRangeType.Redius:
                    ret = config.CollisoionRedius / 100.0f;
                    break;
                case ESkillRangeType.RangeEx:
                    ret = config.AttackRangeEx / 100.0f;
                    break;
                case ESkillRangeType.AfterField:
                    ret = config.AttackRangeAfterFired / 100.0f;
                    break;
            }
            return ret;
        }

        public static void InitAnimationComponents(GameObject target,int uid)
        {
            if (target == null)
                return;
            ComponentAnimation animData = SkillController.Component.GetComponent<ComponentAnimation>(uid);
            animData.InitAnimation(target);
            ComponentTransform transform = SkillController.Component.GetComponent<ComponentTransform>(uid);
            transform.RootGameObj = target;
            transform.RootGameObjTransform = target.transform;
        }
    }

}