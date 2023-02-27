using BulletConfig;
using GameFrame.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 子弹控制器
    /// </summary>
    public class BulletCtrl : CtrlItem
    {
        private List<BulletItem> m_Bullets;

        public override bool IsEnd
        {
            get { return m_Bullets.Count <= 0; }
        }

        public BulletCtrl(SkillDisplayManager manager):base(manager)
        {
            m_Bullets = new List<BulletItem>();
        }

        public void CreateBullet(int id,Vector3 position,Vector3 dir)
        {
            bullet_data bullet = SkillController.SkillCfg.Get<bullet_data>(id);
            if (bullet == null) return;

            BulletItem item = ObjectPool<BulletItem>.Get();
            item.Init(bullet,position,dir);

            m_Bullets.Add(item);
        }

        public override void Update(float deltaTime)
        {
            for(int i= m_Bullets.Count - 1; i >= 0; i++)
            {
                BulletItem item = m_Bullets[i];
                if(item.IsOver)
                {
                    m_Bullets.Remove(item);
                    item.Release();
                    continue;
                }
                item.Update(deltaTime);
            }
        }

        public override void Release()
        {
            foreach(var item in m_Bullets)
            {
                item.Release();
            }
            m_Bullets.Clear();
        }
    }
}
