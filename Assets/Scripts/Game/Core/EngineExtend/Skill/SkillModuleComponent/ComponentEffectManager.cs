using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFrame.Pool;
using EffectConfig;

namespace GameFrame.Skill.Component
{
    public class ComponentEffectManager : PoolClass
    {
        public List<ComponentEffect> ControllEffects = new List<ComponentEffect>();
        
        public float UnitScale = 1.0f;
        
        private uint m_uidBase = 0;

        //管理同一个受击目标的特效
        public Dictionary<int, ComponentEffect> BeCommonAttackEffect = new Dictionary<int, ComponentEffect>();


        public uint AddEffect(int EffectId,int attachEntity,Vector3 offset = default(Vector3),bool beattackEffect= false,Quaternion dirOffset = default(Quaternion))
        {
            uint uid = GetUID();
            effect_data effectCfg = SkillController.SkillCfg.Get<effect_data>(EffectId);
            if(effectCfg == null)
            {
                Debug.LogErrorFormat("get config failed,id:{0}",EffectId);
                return 0;
            }

            if (beattackEffect && ContainsEffectNotLong(EffectId))
                return 0;

            ComponentEffect effect = ObjectPool<ComponentEffect>.Get();
            effect.EffectCfg = effectCfg;
            effect.AttachEntity = attachEntity;
            effect.EffectElapse = 0;
            effect.EffectObj = null;
            effect.EffectObjTrans = null;
            effect.EffectOffset = offset;
            effect.DirOffset = dirOffset;
            effect.EffectUID = uid;
            effect.SkillOverRemove = !effectCfg.NotRemoveOnSkillOver;
            effect.SkillActionOverRemove = effectCfg.RemoveOnSkillActionOver;
            effect.Valid = true;
            effect.Inited = false;
            ControllEffects.Add(effect);

            if(beattackEffect)
            {
                CheckSameAttachEntityBeAttackEffect(attachEntity,effect);
            }

            return uid;

        }

        //后加的优先级高
        private void CheckSameAttachEntityBeAttackEffect(int attachEntity,ComponentEffect curEffect)
        {
            ComponentEffect effect = null;
            if (BeCommonAttackEffect.TryGetValue(attachEntity, out effect))
            {
                if (curEffect.EffectCfg != null && effect.EffectCfg != null && curEffect.EffectCfg.Priorty > effect.EffectCfg.Priorty)
                {
                    RecycaleGameObject(curEffect);
                    ControllEffects.Remove(curEffect);
                }
                else
                {
                    RecycaleGameObject(effect);
                    ControllEffects.Remove(effect);
                    BeCommonAttackEffect[attachEntity] = curEffect;
                }
            }
            else
                BeCommonAttackEffect.Add(attachEntity, curEffect);
        }

        private void RecycaleGameObject(ComponentEffect effect)
        {
            if(effect.EffectObj != null)
            {
                Hooks.ResourceManager.Recycle(effect.EffectObj);
            }
        }

        public void RemoveControlEffectAt(int index)
        {
            if (index >= ControllEffects.Count)
                return;
            ComponentEffect effect = ControllEffects[index];
            ComponentEffect comEffect = null;
            if(BeCommonAttackEffect.TryGetValue(effect.AttachEntity,out comEffect))
            {
                if(effect == comEffect)
                {
                    BeCommonAttackEffect.Remove(effect.AttachEntity);
                }
            }
            RecycaleGameObject(effect);
            ControllEffects.Remove(effect);
        }

        public ComponentEffect GetControlEffectByUID(uint effectUid)
        {
            for(int i=0;i< ControllEffects.Count;i++)
            {
                if(ControllEffects[i].EffectUID ==effectUid)
                {
                    return ControllEffects[i];
                }
            }
            return null;
        }

        //还没加载好就回收没处理
        public void RemoveEffectByUID(uint effectUid)
        {
            for(int i=0;i<ControllEffects.Count;i++)
            {
                if(ControllEffects[i].EffectUID == effectUid)
                {
                    ControllEffects[i].Valid = false;
                    break;
                }
            }
        }

        public void OnskillOverRemvoeEffects()
        {
            for(int i=0;i<ControllEffects.Count;i++)
            {
                if(ControllEffects[i].SkillActionOverRemove)
                {
                    ControllEffects[i].Valid = false;
                    continue;
                }
            }
        }

        public void OnActionOverRemoveEffects()
        {
            for(int i=0;i<ControllEffects.Count;i++)
            {
                if(ControllEffects[i].SkillActionOverRemove)
                {
                    ControllEffects[i].Valid = false;
                    continue;
                }
            }
        }

        public void SetUnitScale(float scale)
        {
            if (scale == 0) scale = 1.0f;
            UnitScale = Mathf.Clamp(scale,0.1f,10.0f);
        }


        private bool ContainsEffectNotLong(int EffctId)
        {
            //for(int i=0;i<ControlEffects.Count;i++)
            //{
            //    if(null!= ControlEffects[i].EffectCfg && ControlEffects[i].EffectCfg.Id == EffctId
            //        && ControlEffects[i].EffectElapse<0.6f)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        private uint GetUID()
        {
            uint uid = m_uidBase++;
            if (uid == 0) uid = m_uidBase++;
            return uid;
        }

        public override void OnRelease()
        {
            for(int i= 0;i< ControllEffects.Count;i++)
            {
                RecycaleGameObject(ControllEffects[i]);
            }

            ControllEffects.Clear();
            m_uidBase = 0;
            UnitScale = 1.0f;
            BeCommonAttackEffect.Clear();
            base.OnRelease();
        }
    }

}