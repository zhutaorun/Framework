using EffectConfig;
using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using TrapConfig;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    public class TrapItem : BaseItem
    {
        public trap_data Config;
        public float TotalLife;
        public float RateTime;
        public float LifeTime;
        public EffectItem Effect;
        private GameObject m_Root;

        public bool IsOver;
        private float intervalTime;

        public override void OnUse()
        {
            base.OnUse();
        }

        public void Init(trap_data cfg,Vector3 position,Vector3 dir)
        {
            if(!m_Root)
            {
                m_Root = new GameObject();
            }

            IsOver = false;
            m_Root.name = cfg.Name + cfg.Id;
            Config = cfg;
            LifeTime = 0;
            TotalLife = ConfigUtil.ConvertTime(cfg.TrapLife);
            RateTime = ConfigUtil.ConvertTime(cfg.TrapRate);
            intervalTime = 0;
            m_Root.transform.position = position;
            m_Root.transform.LookAt(position - dir);
            if(Config.TrapLoopEffect >0)
            {
                effect_data effect = SkillController.SkillCfg.Get<effect_data>((int)Config.TrapLoopEffect);
                if(effect != null)
                {
                    Vector3 offset = effect.OnEntity ? Vector3.zero:m_Root.transform.position;
                    Effect = GetCtrl<EffectCtrl>().CreateEffect((int)Config.TrapLoopEffect, false, offset, m_Root.transform);
                }
            }
        }

        public void Update(float deltaTime)
        {
            if(LifeTime <=0)
            {
                TriggerEvent(EOutputTime.OnStart);
                if (Config.TrapStartEffect > 0)
                {
                    effect_data effect = SkillController.SkillCfg.Get<effect_data>((int)Config.TrapStartEffect);
                    if (effect != null)
                    {
                        Vector3 offset = effect.OnEntity ? Vector3.zero : m_Root.transform.position;
                        GetCtrl<EffectCtrl>().CreateEffect((int)Config.TrapStartEffect, true, offset, m_Root.transform);
                    }
                }
            }
            LifeTime += deltaTime;
            if (LifeTime >= TotalLife)
            {
                TriggerEvent(EOutputTime.OnEnd);
                if (Config.TrapEndEffect > 0)
                {
                    effect_data effect = SkillController.SkillCfg.Get<effect_data>((int)Config.TrapEndEffect);
                    if (effect != null)
                    {
                        Vector3 offset = effect.OnEntity ? Vector3.zero : m_Root.transform.position;
                        GetCtrl<EffectCtrl>().CreateEffect((int)Config.TrapEndEffect, true, offset, m_Root.transform);
                    }
                }
                IsOver = true;
            }
            else if (Config.TrapRate > 0)
            {
                intervalTime -= deltaTime;
                if(intervalTime<=0)
                {
                    intervalTime = RateTime;
                    TriggerEvent(EOutputTime.OnInterval);
                    if(Config.TrapIntervalEffect>0)
                    {
                        effect_data effect = SkillController.SkillCfg.Get<effect_data>((int)Config.TrapIntervalEffect);
                        if (effect != null)
                        {
                            Vector3 offset = effect.OnEntity ? Vector3.zero : m_Root.transform.position;
                            GetCtrl<EffectCtrl>().CreateEffect((int)Config.TrapIntervalEffect, true, offset, m_Root.transform);
                        }
                    }
                }
            }
            if(Effect != null)
            {
                Effect.SimpleEffect(deltaTime);
                Effect.LifeTime += deltaTime;
            }
        }

        private void TriggerEvent(EOutputTime outputTime)
        {
            foreach(var item in Config.TrapOutputs)
            {
                if(item.OutputTime == outputTime)
                {
                    SkillRangeShow.Instance.ShowRange(Config.EventPick,null,m_Root,1);

                    GetCtrl<EventCtrl>().TriggerOutput(item,m_Root.transform);
                }
            }
        }
        public override void OnRelease()
        {
            base.OnRelease();
            if(Effect != null)
            {
                Effect.Release();
                Effect = null;
            }

            if(m_Root)
            {
                GameObject.DestroyImmediate(m_Root);
                m_Root = null;
            }
            Config = null;
            TotalLife = 0;
            LifeTime = 0;
        }
    }
}
