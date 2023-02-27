using GameFrame.Skill.Component;
using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.GameSystem
{
    public class SystemSkill : ISystem
    {
        public void Update(float delTime)
        {
            Component<ComponentSkill> components = SkillController.Component.GetComponents<ComponentSkill>();
            if (components != null)
            {
                ComponentSkill component;
                for (int i = 0, imax = components.DataCount; i < imax; i++)
                {
                    component = components.GetComponent(i);
                    if (component == null)
                        continue;
                    UpdateSkill(i, component, delTime);
                }
            }
        }

        private void UpdateSkill(int key, ComponentSkill skill, float delTime)
        {
            if (skill.ActionChangeDirDirty)
            {
                DoSkillAction(key, skill);
                skill.ActionDirty = false;
            }
            if (skill.ActionChangeDirDirty)
            {
                //int curIndx = -1;
                //string actionName = GetCurActionName(entity,skill,ref curIndex);
                //if(curIndx == -1 || curIndx != skill.CurActionAnimIndex)
                //{
                //    if(skill.CurAnimName!= string.Empty && skill.CurAnimName != actionName)
                //    {
                //        skill.CurAnimName = actionName;
                //        skill.CurActionAnimIndex = curIndx;
                //        animationComponent.SetStateAnimation(actionName,0.1f,0,true,true);
                //    }
                //}
                skill.ActionChangeDirDirty = false;
            }
            UpdateSkillElapse(key, delTime, skill);
        }

        private void UpdateSkillElapse(int key, float delTime, ComponentSkill skillComponent)
        {
            if (skillComponent.CurSkillCfg != null && skillComponent.CurSkillCfg.SkillActionList.Count > 0 && skillComponent.CurActNo < skillComponent.CurSkillCfg.SkillActionList.Count)
            {
                if (skillComponent.SkillOver)
                {
                    int skillId = skillComponent.CurSkillCfg.Id;
                    skillComponent.SetSkillActionRefresh(0, 0, 0, 1, 0, false, false);
                    SkillController.Output.SkillOver(key, skillComponent.TriggerParam.heroId, skillId);
                    return;
                }
                skillComponent.CurActElapsedTime += delTime * 1000 * skillComponent.CurActionSpeed;
                for (int i = skillComponent.CurActNextEventIdx, imax = skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents.Count; i < imax; i++)
                {
                    float time = skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents[i].EventTimePoint;
                    if (time < skillComponent.CurActStartPoint * 1000)
                        continue;
                    if (skillComponent.CurActElapsedTime >= time)
                    {
                        OnEvent(key, skillComponent, skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents[i], skillComponent.CurActNo + i);
                        skillComponent.CurActNextEventIdx = i + 1;
                    }
                }
                if (skillComponent.AutoNext)
                {
                    //为了提前结束动画，好让下一个动画做混合，不然现在技能放完切下一个动画会硬切
                    float endRate = 1f;
                    if (skillComponent.CurActElapsedTime == skillComponent.CurSkillCfg.SkillActionList.Count - 1)
                    {
                        endRate = 0.9f;
                    }
                    if (skillComponent.CurActElapsedTime >= skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].ActionTime * endRate)
                    {
                        if (endRate < 1f)
                        {
                            //如果提前结束了，把中间事件也补一下
                            for (int i = skillComponent.CurActNextEventIdx, imax = skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents.Count; i < imax; i++)
                            {
                                float time = skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents[i].EventTimePoint;
                                if (time < skillComponent.CurActStartPoint * 1000)
                                    continue;
                                if (skillComponent.CurActElapsedTime >= time * endRate)
                                {
                                    OnEvent(key, skillComponent, skillComponent.CurSkillCfg.SkillActionList[skillComponent.CurActNo].FrameEvents[i], skillComponent.CurActNo + i);
                                    skillComponent.CurActNextEventIdx = i + 1;
                                }
                            }
                        }

                        uint exId = SkillController.ID.GetExternalID(key);
                        if (skillComponent.CurActNo + 1 < skillComponent.CurSkillCfg.SkillActionList.Count)
                        {
                            SkillController.Output.SkillActionOver((int)exId, skillComponent.CurSkillCfg.Id, skillComponent.CurActNo);

                            skillComponent.SetSkillActionRefresh(skillComponent.CurSkillCfg.Id, skillComponent.CurActNo + 1, 0, skillComponent.CurActionSpeed, 0, false, skillComponent.AutoNext);
                        }
                        else
                        {
                            int skillId = skillComponent.CurSkillCfg.Id;
                            skillComponent.SetSkillActionRefresh(0, 0, 0, 1, 0, false, false);
                            SkillController.Output.SkillOver((int)exId, skillComponent.TriggerParam.heroId, skillId);
                        }

                    }
                }
            }
        }

        private void OnEvent(int entity,ComponentSkill skillComponent,FrameEvent frameInfo,int index)
        {
            switch(frameInfo.EventType)
            {
                case EFrameEventType.Effect:
                    //Hooks.GlobalEvent.Trigger<int, FrameEvent, int>(GameFrame.EventEnum, SkillEditor_FrameEvent_Effect, skillComponent.TriggerParam.heroId, frameInfo, entity);
                    break;
                case EFrameEventType.SetBuff:
                    break;
                case EFrameEventType.DirectHurt:
                    break;
                case EFrameEventType.SendBullet:
                    break;
                case EFrameEventType.CameraShake:
                    break;
                case EFrameEventType.SetClientMove:
                    break;
                case EFrameEventType.Sound:
                    break;
            }
        }

        private void DoSkillAction(int key,ComponentSkill skill)
        {
            if(skill.CurSkillCfg == null || skill.CurSkillCfg.Id != skill.CurSkillId)
            {
                skill.CurSkillCfg = SkillController.SkillCfg.Get<skillnew_data>(skill.CurSkillId);
            }
            if (skill.CurSkillCfg == null) return;
            //设置
            if (skill.CurSkillCfg.SkillActionList[skill.CurActNo].ActionSpeed > 0)
                skill.CurActionSpeed = skill.CurSkillCfg.SkillActionList[skill.CurActNo].ActionSpeed / 100.0f;
            skill.CurActElapsedTime = skill.CurActStartPoint * 1000;
            int curIndex = -1;
            string actionName = GetCurActionName(key,skill,ref curIndex);
            if(curIndex == -1|| curIndex != skill.CurActionAnimIndex)
            {
                int fadeTime = skill.CurSkillCfg.SkillActionList[skill.CurActNo].AnimCrossTime;
                skill.CurAnimName = actionName;
                skill.CurActionAnimIndex = curIndex;
                SkillController.Component.GetComponent<ComponentAnimation>(key).SetStateAnimation(skill.CurAnimName,skill.CurActNo == 0?(fadeTime<=0?0.2f:fadeTime/1000.0f):(fadeTime/1000.0f),skill.CurActStartPoint,true,true,skill.CurActionSpeed);
            }
        }

        private string GetCurActionName(int entity,ComponentSkill skill,ref int actIdx)
        {
            actIdx = -1;
            string ret = skill.CurSkillCfg.SkillActionList[skill.CurActNo].ActionPath;

            return ret;
        }
    }
}