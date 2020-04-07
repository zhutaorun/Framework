using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassPool
{
    public class ClassObjectPool<T>:PoolBase where T:PoolClass,new ()
    {
        private static ClassObjectPool<T> instance;

        static ClassObjectPool()
        {
        }

        public ClassObjectPool()
        {
        }

        public static T Get()
        {
            if(ClassObjectPool<T>.instance == null)
            {
                ClassObjectPool<T>.instance = new ClassObjectPool<T>();
                PoolBase.objectPools.Add(ClassObjectPool<T>.instance);
            }
            int iCount = ClassObjectPool<T>.instance.pool.Count;
            if(iCount<=0)
            {
                T t = Activator.CreateInstance<T>();
                t.holder = ClassObjectPool<T>.instance;
                t.OnUse();
                return t;
            }
            T item = (T)ClassObjectPool<T>.instance.pool[iCount - 1];
            ClassObjectPool<T>.instance.pool.RemoveAt(iCount-1);
            item.holder = ClassObjectPool<T>.instance;
            item.OnUse();
            return item;
        }

        public override void Release(PoolClass obj)
        {
            pool.Add(obj);
        }

        public override void Clear()
        {
            if(ClassObjectPool<T>.instance !=null)
            {
                ClassObjectPool<T>.instance.pool.Clear();
                ClassObjectPool<T>.instance = null;
            }
        }
    }
}
