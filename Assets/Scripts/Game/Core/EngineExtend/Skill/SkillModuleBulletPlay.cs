using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
using System;
using UnityEngine;

public partial class SkillModuleMain : SkillInterface
{
    public void PlayBullet(int id,int bulletId,Vector3 offset,SkillTriggerParam skillParam,Action<int,SkillTriggerParam> callback = null)
    {
        int entity = SkillController.ID.ReqModuleId((uint)id);
        bool ret = UtilityBullet.CreateBullet(entity,bulletId,offset,skillParam,callback);
        if (!ret)
            SkillController.ID.ReleaseModuleId(entity);
    }

    public void PlayBullet(int bulletId, Vector3 offset, SkillTriggerParam skillParam, Action<int, SkillTriggerParam> callback = null)
    {
        int entity = SkillController.ID.ReqModuleId();
        bool ret = UtilityBullet.CreateBullet(entity, bulletId, offset, skillParam, callback);
        if (!ret)
            SkillController.ID.ReleaseModuleId(entity);
    }
}
