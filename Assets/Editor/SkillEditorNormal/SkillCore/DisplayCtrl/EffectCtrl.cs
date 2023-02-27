using EffectConfig;
using GameFrame.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 特效控制、加载
    /// </summary>
    public class EffectCtrl : CtrlItem
    {
        private List<EffectItem> m_CtrlEffects;

        public override bool IsEnd { get { return m_CtrlEffects.Count <= 0; } }

        public Transform EffectRoot { get { return BindInfo == null ? null : BindInfo.EffectParent; } }

        public EffectCtrl(SkillDisplayManager manager):base(manager)
        {
            m_CtrlEffects = new List<EffectItem>();
        }

        public EffectItem CreateEffect(int id,bool autoRelease,Vector3 offset,Transform parent)
        {
            effect_data config = SkillController.SkillCfg.Get<effect_data>(id);
            if (config == null) return null;
            EffectItem item = ObjectPool<EffectItem>.Get();
            item.Init(config, autoRelease, offset, parent);

            if(autoRelease)
            {
                m_CtrlEffects.Add(item);
            }

            return item;
        }

        public override void Update(float deltaTime)
        {
            for(int i=m_CtrlEffects.Count-1;i>=0;i--)
            {
                EffectItem item = m_CtrlEffects[i];
                if(item.LifeTime < item.Config.LifeTime)
                {
                    item.SimpleEffect(deltaTime);
                    item.LifeTime += deltaTime;
                }
                else
                {
                    m_CtrlEffects.Remove(item);
                    item.Release();
                }
            }
        }

        public override void Release()
        {
            foreach(var item in m_CtrlEffects)
            {
                item.Release();
            }
            m_CtrlEffects.Clear();
        }
    }
}
