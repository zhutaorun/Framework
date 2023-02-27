using GameFrame.Skill.Component;

/// <summary>
/// 输出事件
/// </summary>
public partial class SkillPlayOutput : SkillInterfaceOutput
{
    public void SkillOver(int entity,int heroId,int skillId)
    {
        ComponentEffectManager effectComponent = SkillController.Component.GetComponent<ComponentEffectManager>(entity);
        if(effectComponent != null)
        {
            effectComponent.OnskillOverRemvoeEffects();
        }
        //GameFrame.Hooks.GlobalEvent.Trigger<int, int>(GameFrame.EventEnum.SkillEditor_SkillOver,heroId,skillId);
    }

    public void SkillActionOver(int entityId,int skillId,int actNo)
    {

    }
}
