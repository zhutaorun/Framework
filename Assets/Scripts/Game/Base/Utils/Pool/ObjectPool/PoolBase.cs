namespace GameFrame.Pool
{
    using System.Collections.Generic;
    public abstract class PoolBase : IPool
    {
        /// <summary>
        /// 所有对象池列表
        /// </summary>
        public static List<PoolBase> SM_ObjectPools = new List<PoolBase>();

        protected List<object> m_Pool = new List<object>();

        public static void ClearAllObectPool()
        {
            for(int i=0;i<SM_ObjectPools.Count;i++)
            {
                SM_ObjectPools[i].Clear();
            }
            SM_ObjectPools.Clear();
        }
        public abstract void Release(PoolClass obj);
        public abstract void Clear();
    }
}
