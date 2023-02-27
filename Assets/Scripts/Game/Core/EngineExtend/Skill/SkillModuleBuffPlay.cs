using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkillModuleMain : SkillInterface
{
    public void PlayBuff(int id,int attachEntityId,GameObject attachEntity,int buffId,SkillTriggerParam triggerParam = default(SkillTriggerParam))
    {
        int entityId = SkillController.ID.ReqModuleId((uint)id);
        bool ret = UtilityBuff.CreateBuff(entityId,attachEntityId,attachEntity,buffId,triggerParam);
        if (!ret)
            SkillController.ID.ReleaseModuleId(entityId);
    }

    public void BuffOver(int entityId,int buffId)
    {
        entityId = SkillController.ID.GetModuleId((uint)entityId);
        if(entityId<0)
        {
            Debug.LogError("BuffOver error id not find!buffid="+buffId+"  "+"entityId"+entityId);
            return;
        }
        UtilityBuff.BuffOver(entityId,buffId);
    }
}
