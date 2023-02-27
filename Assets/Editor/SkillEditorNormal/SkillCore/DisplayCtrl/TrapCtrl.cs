using GameFrame.Pool;
using System.Collections.Generic;
using TrapConfig;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 陷阱控制器
    /// </summary>
    public class TrapCtrl : CtrlItem
    {
        private List<TrapItem> m_Traps;

        public override bool IsEnd { get { return m_Traps.Count <= 0; } }

        public TrapCtrl(SkillDisplayManager manager):base(manager)
        {
            m_Traps = new List<TrapItem>();
        }

        public void CreateTrap(int id,Vector3 position,Vector3 dir)
        {
            trap_data cfg = SkillController.SkillCfg.Get<trap_data>(id);
            if (cfg == null) return;

            TrapItem item = ObjectPool<TrapItem>.Get();

            item.Init(cfg,position,dir);

            m_Traps.Add(item);
        }

        public override void Update(float deltaTime)
        {
            for(int i = m_Traps.Count-1;i>=0;i--)
            {
                TrapItem trap = m_Traps[i];
                if(trap.IsOver)
                {
                    m_Traps.Remove(trap);
                    trap.Release();
                    continue;
                }
                trap.Update(deltaTime);
            }
        }

        public override void Release()
        {
            foreach(var item in m_Traps)
            {
                item.Release();
            }
            m_Traps.Clear();
        }
    }
}