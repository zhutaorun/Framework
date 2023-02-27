using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;

namespace GameFrame.Skill.GameSystem
{
    public class SystemTrap : ISystem
    {
        public void Update(float delTime)
        {
            //ComponentTrap trapComponent;
            //ComponentEffectManager effectManager;
            //Component<ComponentTrap> components = SkillController.Component.GetComponents<ComponentTrap>();
            //Component<ComponentEffectManager> eftcomponents = SkillController.Component.GetComponents<ComponentEffectManager>();
            //if(components != null)
            //{
            //    for(int i=0,imax = components.DataCount;i<imax;i++)
            //    {
            //        trapComponent = components.GetComponent(i);
            //        if (trapComponent == null)
            //            continue;
            //        if(trapComponent.Overed)
            //        {
            //            trapComponent.Inited = true;
            //            trapComponent.Overed = false;
            //            if(trapComponent.TrapLoopEffectCount >0)
            //            {
            //                effectManager = eftcomponents.GetComponent(i);
            //                for(int value =0;value<trapComponent.TrapLoopEffectCount;value++)
            //                {
            //                    effectManager.RemoveEffectByUID(trapComponent.TrapLoopEffectUId[value]);
            //                }
            //            }
            //        }
            //        if(!trapComponent.Inited)
            //        {
            //            if(trapComponent.TrapConfig != null && trapComponent.TrapConfig.TrapLoopEffect>0)
            //            {
            //                uint uid= UtilityEffect.PlayEffect(i, i, (int)trapComponent.TrapConfig.TrapLoopEffect);
            //                if(uid!= 0)
            //                {
            //                    trapComponent.TrapLoopEffectUId[trapComponent.TrapLoopEffectCount] = uid;
            //                    trapComponent.TrapLoopEffectCount++;
            //                    Debug.Assert(trapComponent.TrapLoopEffectCount<10);
            //                }
            //            }
            //            trapComponent.Inited = true;
            //        }
            //        DebugTrapRange(i, trapComponent);
            //    }
            //}
        }

        //private void DebugTrapRange(int entity,ComponentTrap trapComponent)
        //{
        //    if (!SkillDefine.DEBUG_SHOW_TRAP_RANGE) return;
        //    ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransform>(entity);
        //    SkillRangeShow.Instance.ShowRange(trapComponent.TrapConfig.EventPick,null,trans.RootGameObj);
        //}
    }
}
