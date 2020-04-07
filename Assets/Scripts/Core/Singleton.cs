using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameFrame
{
    public abstract class Singleton<T> where T :Singleton<T>,new ()
    {
        static object       _lock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if(_instance ==null)
                {
                    _instance = new T();
                    System.Threading.Thread.MemoryBarrier();
                    _instance.InitSingeton();
                }

                return _instance;
            }
        }


        public virtual void InitSingeton()
        {

        }

        virtual public void Release()
        {
            _instance = default(T);
        }
    }


    public class SingletonMBAuto<T> : MonoBehaviour where T : Component
    {
        private static bool _isApplicationQuit = false;
        private static T _Instance;

        public bool IsALive
        {
            get
            {
                if(_Instance == null)
                {
                    return false;
                }
                return !_isApplicationQuit;
            }
        }

        public static T Instance
        {
            get
            {
                if(_Instance == null && !_isApplicationQuit)
                {
                    _Instance = FindObjectOfType(typeof(T)) as T;
                    if(_Instance== null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name.ToString();
                        _Instance = (T)obj.AddComponent(typeof(T));
                    }
                    if (Application.isPlaying)
                        GameObject.DontDestroyOnLoad(_Instance);
                }
                return _Instance;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            //应用程序退出标记
            _isApplicationQuit = true;
        }

        private void OnDestroy()
        {
            //应用程序退出标记
            if (_Instance!=null)
            {
                _isApplicationQuit = true;
                _Instance = null;
            }
        }
    }
}
