using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    public interface BaseCtrl 
    {
        void Release();

        void Update(float deltaTime);
    }

    public class CtrlItem:BaseCtrl
    {
        private SkillDisplayManager m_Manage;

        public CtrlItem(SkillDisplayManager manager)
        {
            m_Manage = manager;
        }

        public virtual bool IsEnd { get { return true; } }

        public virtual void Release()
        {

        }


        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {

        }
            
        /// <summary>
        /// 获取Ctrl
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCtrl<T>() where T: class
        {
            return m_Manage.GetCtrl<T>();
        }

        /// <summary>
        /// 技能是否在播放中
        /// </summary>
        public bool Playing
        {
            get
            {
                return m_Manage.IsPlaying;
            }
        }

        public MonoBindInfo BindInfo
        {
            get
            {
                return m_Manage.BindInfo;
            }
        }
    }
        
        
}
