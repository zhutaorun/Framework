using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkillModuleMain : SkillInterface
{

    private bool _isInit = false;

    public void Init()
    {
        if (_isInit) return;
        _isInit = true;
        SkillController.Init();
    }

    public void Release()
    {
        //清空所有数据
        if (!_isInit) return;
        _isInit = false;
        SkillController.Clear();
    }

    public void Update(float delTime)
    {
        SkillController.System.Update(delTime);
    }

    public void OnEntityRemove(int entityId)
    {
        //清空entity数据
        int id = SkillController.ID.GetModuleId((uint)entityId);
        if(id>=0)
        {
            SkillController.ID.ReleaseModuleId(id);
            SkillController.Component.RemoveEntityComponent(id);
        }
    }

    public void UpdateEntityPos(int entity,Vector3 pos)
    {
        entity = SkillController.ID.ReqModuleId((uint)entity);
        ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransform>(entity,false);
        if (trans != null)
            trans.RootGameObjTransform.position = pos;
    }

    public void PlayAction(GameObject target,int entityId,string actionName,float fadeTime,float speed =1.0f)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        UtilitySkill.InitAnimationComponents(target,entityId);
        ComponentAnimation componentAnimation = SkillController.Component.GetComponent<ComponentAnimation>(entityId);
        float actionStartPoint = 0;
        componentAnimation.SetStateAnimation(actionName,fadeTime,actionStartPoint,false,true,speed);
    }

    public bool IsPlayAction(int entityId,string actionName)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        ComponentAnimation componentAnimation = SkillController.Component.GetComponent<ComponentAnimation>(entityId);

        if (componentAnimation.CurAnimName == actionName
            && componentAnimation.CurAnimation[actionName] != null
            && componentAnimation.CurAnimation.isPlaying)
            return true;
        return false;
    }

    public void PlayActionFromPointTime(GameObject target,int entityId,string actionName,float fadeTime,float actionStartPoint,float speed= 1.0f)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        UtilitySkill.InitAnimationComponents(target,entityId);
        ComponentAnimation componentAnimation = SkillController.Component.GetComponent<ComponentAnimation>(entityId);
        componentAnimation.SetStateAnimation(actionName,fadeTime,actionStartPoint,false,true,speed);
    }

    public bool IsActionEnd(int entityId,string actionName,float normalizedTime = 0.85f)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        ComponentAnimation componentAnimation = SkillController.Component.GetComponent<ComponentAnimation>(entityId);
        if (componentAnimation == null)
            return false;
        if (!componentAnimation.CurAnimName.Equals(actionName))
            return true;
        AnimationState state = componentAnimation.CurAnimation[actionName];
        if(state == null)
        {
            return false;
        }
        else
        {
            return state.normalizedTime >= normalizedTime;
        }
    }


    public bool IsSkillEnd(int entityId,int skillId)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        ComponentSkill data = SkillController.Component.GetComponent<ComponentSkill>(entityId);
        ComponentAnimation componentAnimation = SkillController.Component.GetComponent<ComponentAnimation>(entityId);
        if (data == null || componentAnimation == null)
            return false;
        if (data.CurSkillId != skillId || !componentAnimation.CurAnimIsSkill)
            return true;
        AnimationState state = componentAnimation.CurAniState;
        if(state == null)
        {
            return false;
        }
        else
        {
            return state.normalizedTime > 0.9f;
        }
    }

    public void PlaySkill(GameObject self,int entityId,int skillId,GameObject target = null,int targetEntityid = 0,Vector3 dir = default(Vector3),Vector3 pos = default(Vector3),int actNo = 0,int point = 0,bool autoNext = true,float speed = 1.0f,float unitScale =1.0f)
    {
        int uid = entityId;
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        ComponentEffectManager effectComponent = SkillController.Component.GetComponent<ComponentEffectManager>(entityId);
        if (effectComponent != null)
            effectComponent.SetUnitScale(unitScale);
        ComponentSkill data = SkillController.Component.GetComponent<ComponentSkill>(entityId);
        data.SetSkillActionRefresh(skillId,actNo,point,speed,0,false,autoNext);
        data.SetSkillTriggerParam(self,target,uid,targetEntityid,dir,pos);
        UtilitySkill.InitAnimationComponents(self,entityId);
    }

    public void StopSkill(int entityId)
    {
        entityId = SkillController.ID.ReqModuleId((uint)entityId);
        ComponentSkill data = SkillController.Component.GetComponent<ComponentSkill>(entityId);
        data.ForceOver();
    }
   
   


    

    
}
