using Boo.Lang;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 通用容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Container<T>
    {
        private readonly Dictionary<string, T> _instances = new Dictionary<string, T>();


        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="overwriteExisting"></param>
        public void Add(T instance,bool overwriteExisting  = true)
        {
            string name = instance.GetType().FullName;
            if(_instances.ContainsKey(name))
            {
                _instances.Add(name,instance);
            }
            else if(overwriteExisting)
            {
                _instances[name] = instance;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Remove(T instance)
        {
            string name = instance.GetType().FullName;
            if(_instances.ContainsKey(name))
            {
                _instances.Remove(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="ItemT"></typeparam>
        /// <returns></returns>
        public ItemT Get<ItemT>()  where ItemT : T
        {
            var key = typeof(ItemT).FullName;
            if(key == null)
            {
                return default(ItemT);
            }

            return (ItemT)_instances[key];
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _instances.Clear();
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _instances.Count;
        }


        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,T>.Enumerator GetEnumerator()
        {
            return _instances.GetEnumerator();
        }

        /// <summary>
        /// 获取服务接口名
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public string[] GetInterfaceNames(object instance)
        {
            var ret = instance.GetType().FindInterfaces((type, criteria) =>
            type.GetInterfaces().Any(t => t.FullName == typeof(T).FullName),instance).Select(type =>type.FullName).ToArray();
            return ret;
        }

    }
}