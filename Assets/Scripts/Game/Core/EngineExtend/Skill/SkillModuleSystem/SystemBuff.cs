using BuffConfig;
using GameFrame.Skill.Component;
using SkillmoveConfig;
using GameFrame.Skill.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.GameSystem
{
    public class SystemBuff : ISystem
    {
        public void Update(float delTime)
        {
            ComponentBuff buffComponent;
            Component<ComponentBuff> components = SkillController.Component.GetComponents<ComponentBuff>();
            if(components != null)
            {
                for(int i=0,imax = components.DataCount;i<imax;i++)
                {
                    buffComponent = components.GetComponent(i);
                    if (buffComponent == null)
                        continue;
                    if(!buffComponent.Inited)
                    {
                        OnStartBuff(i,buffComponent);
                        buffComponent.Inited = true;
                    }
                    OnUpdateBuff(i, buffComponent);
                }
            }
        }

        private void OnStartBuff(int entity,ComponentBuff buffComponent)
        {
            //处理位移
            //if(buffComponent.BuffConfig.BuffType == BuffConfig.EBuffType.move)
            //{
            //    ProcessMoveBuffStart(entity,buffComponent);
            //}
            //特效显示
            if(buffComponent.BuffConfig.BuffStartEffects !=null)
            {
                for (int value = 0; value < buffComponent.BuffConfig.BuffStartEffects.Count; value++)
                    UtilityEffect.PlayEffect(entity,buffComponent.AttachEntity,(int)buffComponent.BuffConfig.BuffStartEffects[value]);
            }
            buffComponent.NeedCheckBuffLoopEffect = true;
            CheckPlayLoopEffect(entity, buffComponent);
        }

        private void CheckPlayLoopEffect(int entity,ComponentBuff buffComponent)
        {
            if (!buffComponent.NeedCheckBuffLoopEffect)
                return;
            bool canplay = true;
            if(buffComponent.NeedCheckBuffLoopEffect)
            {
                //switch(buffComponent.BuffConfig.BuffType)
                //{
                //    case EBuffType.frozen:
                //        if(!buffComponent.LogicNtfPlayLoopEffect)
                //        {
                //            canplay = false;
                //        }
                //        break;
                //}

            }
            if (!canplay)
                return;
            buffComponent.NeedCheckBuffLoopEffect = false;
            if(buffComponent.BuffConfig.BuffLoopEffects != null)
            {
                for(int value =0;value<buffComponent.BuffConfig.BuffLoopEffects.Count;value++)
                {
                    uint uid = UtilityEffect.PlayEffect(entity, buffComponent.AttachEntity, (int)buffComponent.BuffConfig.BuffLoopEffects[value]);
                }
            }
        }

        /// <summary>
        /// 处理移动buff,一定
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="buffComponent"></param>
        private void OnUpdateBuff(int entity,ComponentBuff buffComponent)
        {
            UtilityEffect.OnBuffUpdateCheckAnimation(buffComponent.AttachEntity,buffComponent.BuffConfig.BuffType,buffComponent.LogicNtfPlayLoopEffect);
            CheckPlayLoopEffect(entity,buffComponent);
        }

        private void ProcssMoveBuffStart(int entity,ComponentBuff buffComponent)
        {
            skillmove_data skillmove = SkillController.SkillCfg.Get<skillmove_data>((int)buffComponent.BuffConfig.Id);
            if(skillmove!=null)
            {
                if(skillmove.VerticalTime == 0 &&(skillmove.RangeValue<=0 || skillmove.MoveSpeedValue<=0))
                {
                    UtilityBuff.BuffOver(entity, (int)buffComponent.BuffConfig.Id);
                    return;
                }
                float range = skillmove.RangeValue >= 0 ? skillmove.RangeValue / 100.0f : UtilitySkill.GetConfigRangeByType(skillmove.Range, buffComponent.FromSkillParam.SkillCfg);
                Vector3 dir = Vector3.zero;
                ComponentTransform trans = null;
                switch(skillmove.MoveRelayType)
                {
                    case EMoveRelayType.TriggerDir:
                        //技能触发方向
                        dir = buffComponent.FromSkillParam.skillTriggerDir;
                        break;
                    case EMoveRelayType.TriggerPos:
                        break;
                    case EMoveRelayType.TriggerPosOp:
                        break;
                    case EMoveRelayType.TriggerDirOp:
                        dir = -buffComponent.FromSkillParam.skillTriggerDir;
                        break;
                    case EMoveRelayType.SkillCmdTarget:
                        if (buffComponent.FromSkillParam.skillCmdTarget == null)
                        {
                            dir = buffComponent.FromSkillParam.skillTriggerDir;
                        }
                        else
                        {
                            trans = SkillController.Component.GetComponent<ComponentTransform>(buffComponent.AttachEntity);
                            Vector3 cmddisPos = buffComponent.FromSkillParam.skillCmdTarget.transform.position - trans.RootGameObjTransform.position;
                            if(cmddisPos == Vector3.zero)
                            {
                                dir = buffComponent.FromSkillParam.skillTriggerDir;
                            }
                            else
                            {
                                range = cmddisPos.magnitude;
                                dir = cmddisPos.normalized;
                            }
                        }
                        break;
                    case EMoveRelayType.SkillCmdDir:
                        dir = buffComponent.FromSkillParam.skillCmdDir;
                        break;
                    case EMoveRelayType.SkillCmdPos:
                        if(buffComponent.FromSkillParam.skillCmdPos == Vector3.zero)
                        {
                            dir = buffComponent.FromSkillParam.skillTriggerDir;
                        }
                        else
                        {
                            trans = SkillController.Component.GetComponent<ComponentTransform>(buffComponent.AttachEntity);
                            Vector3 cmddisPos = buffComponent.FromSkillParam.skillCmdPos - trans.RootGameObjTransform.position;
                            if(cmddisPos== Vector3.zero)
                            {
                                dir = buffComponent.FromSkillParam.skillTriggerDir;
                            }
                            else
                            {
                                range = cmddisPos.magnitude;
                                dir = cmddisPos.normalized;
                            }
                        }
                        break;
                    case EMoveRelayType.TrapPosOp:
                        break;
                }

                float speed = skillmove.MoveSpeedValue;
                float height = skillmove.FlyHeight / 100.0f;
                int verticalTime = skillmove.VerticalTime;
                if(range>0 || height>0)
                {
                    ComponentMove moveData = SkillController.Component.GetComponent<ComponentMove>(buffComponent.AttachEntity);
                    if (trans == null)
                        trans = SkillController.Component.GetComponent<ComponentTransform>(buffComponent.AttachEntity);
                    moveData.SetBuffMoveCmdData(trans.RootGameObjTransform,skillmove,range,dir,speed,height,verticalTime,()=> 
                    {
                        UtilityBuff.BuffOver(entity,(int)buffComponent.BuffConfig.Id);
                    },buffComponent.FromSkillParam);
                    return;
                }
            }
            UtilityBuff.BuffOver(entity,(int)buffComponent.BuffConfig.Id);
        }
    }

}