using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameFrame
{
    public abstract class SingletonNew<T> where T : class,new()
    {
        private static T _instance;
        static Object _lock = new Object();

        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_lock)
                    {
                        CreateInstance();
                    }
                }
                return _instance;
            }
        }

        public static void CreateInstance()
        {
            if(_instance == null)
            {
                _instance = new T();
                (_instance as SingletonNew<T>).Init();
            }
        }

        public virtual void Init() { }

        public static void DestoryInstance()
        {
            if(_instance != null)
            {
                (_instance as SingletonNew<T>).UnInit();
                _instance = (T)((object)null);
            }
        }

        public virtual void UnInit() { }
    }
}
