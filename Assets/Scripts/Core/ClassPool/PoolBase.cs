using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassPool
{
    /* 对象池基类*/
    public abstract class PoolBase:IPool
    {
        /* 所有对象池列表*/
        public static List<PoolBase> objectPools = new List<PoolBase>();

        /* 对象列表*/
        protected List<object> pool = new List<object>(128);

        public static void ClearAllObjectPool()
        {
            for(int i=0;i<objectPools.Count;++i)
            {
                objectPools[i].Clear();
            }
            objectPools.Clear();
        }

        public abstract void Release(PoolClass obj);

        public abstract void Clear();
    }
}
