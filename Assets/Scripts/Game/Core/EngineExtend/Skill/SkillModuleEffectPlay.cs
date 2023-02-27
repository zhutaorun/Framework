using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
using UnityEngine;

public partial class SkillModuleMain : SkillInterface
{
    public uint PlayEntityEffect(GameObject target,int entityId,int effectId,Vector3 offset= default(Vector3),float scale = 1.0f)
    {
        ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransform>(entityId);
        trans.RootGameObjTransform = target.transform;
        trans.RootGameObj = target;
        ComponentEffectManager effectComponent = SkillController.Component.GetComponent<ComponentEffectManager>(entityId);
        if (effectComponent != null)
            effectComponent.SetUnitScale(scale);
        return UtilityEffect.PlayEffect(entityId, entityId, effectId, offset);
    }

    public uint PlayWorldEffect(int effectId,Vector3 pos,Vector3 dir)
    {
        return UtilityEffect.PlayEffect(-1,-1,effectId,pos,dir);
    }

    public void EntityEffectOver(int entityId,uint effectUid)
    {
        UtilityEffect.RemoveEntityEffect(entityId,effectUid);
    }

    public void WorldEffectOver(uint effectUid)
    {
        UtilityEffect.RemoveWorldEffect(effectUid);
    }
}
