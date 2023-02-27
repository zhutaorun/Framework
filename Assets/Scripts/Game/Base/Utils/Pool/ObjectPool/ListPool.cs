using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Pool
{
    public static class ListPool<T>
    {

        private static List<object> pool;

        static ListPool()
        {
            ListPool<T>.pool = new List<object>();
        }

        public static List<T> Get()
        {
            List<T> list;
            {
                if(ListPool<T>.pool.Count <=0)
                {
                    list = new List<T>();
                }
                else
                {
                    List<T> item = (List<T>)ListPool<T>.pool[ListPool<T>.pool.Count - 1];
                    ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count-1);
                    list = item;
                }
            }

#if UNITY_EDITOR
            if (list.Capacity > 128)
            { 
            }
#endif
            return list;
        }

        public static void Release(List<T> list)
        {
            list.Clear();
#if UNITY_EDITOR
            List<object> list1 = ListPool<T>.pool;
            for(int i=0;i<list1.Count;i++)
            {
                if(ListPool<T>.pool[i] == list)
                {
                    throw new InvalidOperationException("ListPool release error");
                }
            }
#endif
            ListPool<T>.pool.Add(list);
        }
    }
}
