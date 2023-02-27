namespace GameFrame.Pool
{
    using System;
    public class ObjectPool<T>:PoolBase where T :PoolClass,new()
    {
        private static ObjectPool<T> instance;

        static ObjectPool()
        {

        }

        public ObjectPool()
        {

        }

        public static T Get()
        {
            if(instance == null)
            {
                instance = new ObjectPool<T>();
                PoolBase.SM_ObjectPools.Add(instance);
            }

            if(instance.m_Pool.Count <= 0)
            {
                object t = Activator.CreateInstance<T>();
                (t as T).Holder = instance;
                (t as T).OnUse();
                return t as T;
            }

            T item = (T)instance.m_Pool[instance.m_Pool.Count - 1];
            instance.m_Pool.RemoveAt(instance.m_Pool.Count-1);
            item.Holder = instance;
            item.OnUse();
            return item;
        }

        public override void Release(PoolClass obj)
        {
            m_Pool.Add(obj);
        }

        public override void Clear()
        {
            if(instance != null)
            {
                instance.m_Pool.Clear();
                instance = null;
            }
        }
    }
}
