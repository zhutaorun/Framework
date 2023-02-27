using System.Collections;
using System.Collections.Generic;
using GameFrame;
using GameFrame.Skill;
using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
//using TrapConfig;
using UnityEngine;

public partial class SkillModuleMain : SkillInterface
{
    public void PlayTrap(int entity,int trapId,Vector3 pos,Vector3 dir)
    {
        //entity = SkillController.ID.ReqLocalId((uint)entity);
        //trap_data config = SkillController.SkillCfg.Get<trap_data>(trapId);
        //if (config == null)
        //    return;
        //ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransfrom>(entity);
        //trans.RootGameObjTransform.position = pos;
        //trans.RootGameObjTransform.rotation = Quaternion.LookRotation(dir);
        //if (config.TrapStartEffect > 0)
        //    UtilityEffect.PlayEffect(entity, entity, (int)config.TrapStartEffect);
    }
}
