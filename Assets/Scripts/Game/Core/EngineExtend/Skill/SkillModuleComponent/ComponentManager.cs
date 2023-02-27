using System;
using System.Collections.Generic;
using GameFrame.Pool;

namespace GameFrame.Skill.Component
{
    public class ComponentManager
    {
        public ComponentEffectManager WorldEffectComponent;

        private Dictionary<Type, IComponents> ComponentDic = new Dictionary<Type, IComponents>();

        public void Init()
        {
            AddComponents<ComponentAnimation>();
            AddComponents<ComponentSkill>();
            AddComponents<ComponentEffectManager>();
            AddComponents<ComponentTransform>();
            AddComponents<ComponentTrap>();
            AddComponents<ComponentMove>();
            WorldEffectComponent = ObjectPool<ComponentEffectManager>.Get();
        }


        public void Clear()
        {
            foreach(var data in ComponentDic)
            {
                data.Value.Release();
            }
            ComponentDic.Clear();
            if(WorldEffectComponent != null)
            {
                WorldEffectComponent.Release();
                WorldEffectComponent = null;
            }
        }
        private Component<T> AddComponents<T>() where T : PoolClass,new ()
        {
            Type curType = typeof(T);
            IComponents ret;
            if(ComponentDic.TryGetValue(curType,out ret))
            {
                ret = new Component<T>();
                ComponentDic.Add(curType,ret);
            }
            return ret as Component<T>;
        }


        public T GetComponent<T>(int id,bool create = true) where T: PoolClass,new()
        {
            Type curType = typeof(T);
            IComponents components = null;
            if(!ComponentDic.TryGetValue(curType,out components))
            {
                return null;
            }
            Component<T> retCom = components as Component<T>;
            T ret = retCom.GetComponent(id);
            if(ret ==  null && create)
            {
                ret = retCom.OnAddEntity(id);
            }
            return ret;
        }

        public Component<T> GetComponents<T>() where T:PoolClass,new ()
        {
            Type curType = typeof(T);
            IComponents components = null;
            if(!ComponentDic.TryGetValue(curType,out components))
            {
                return null;
            }
            Component<T> retCom = components as Component<T>;
            return retCom;
        }

        public void RemoveEntityComponent(int id)
        {
            foreach(var com in ComponentDic)
            {
                com.Value.OnRemoveEntity(id);
            }
        }
    }

}