using System.Collections.Generic;


namespace GameFrame.Skill.GameSystem
{
    public class SystemManager 
    {
        private List<ISystem> m_SystemList = new List<ISystem>();

        public void Init()
        {
            AddSystem<SystemSkill>();
            AddSystem<SystemAnimation>();
            AddSystem<SystemBuff>();
            AddSystem<SystemTrap>();
            AddSystem<SystemEffectManager>();
            AddSystem<SystemMove>();
        }

        public void Clear()
        {
            m_SystemList.Clear();
        }
        
        public void Update(float delTime)
        {
            for(int i=0,imax = m_SystemList.Count;i<imax;i++)
            {
                m_SystemList[i].Update(delTime);
            }
        }

        private void AddSystem<T>() where T :ISystem,new ()
        {
            T system = new T();
            m_SystemList.Add(system);
        }
    }

}