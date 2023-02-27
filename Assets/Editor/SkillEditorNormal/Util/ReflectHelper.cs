using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 反射辅助类，cache已获取过的对象
    /// </summary>
    public static class ReflectHelper
    {
        /// <summary>
        /// 缓存字段内容
        /// </summary>
        private static Dictionary<Type, Dictionary<string, FieldInfo>> typeFieldDict = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        /// <summary>
        /// 缓存property内容
        /// </summary>
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> typePropertyDict = new Dictionary<Type, Dictionary<string, PropertyInfo>>();


        /// <summary>
        /// 缓存字段获取时的flag cache,下次获取时不匹配的情况下重新获取
        /// </summary>
        private static Dictionary<Type, BindingFlags> fieldTypeFlagDict = new Dictionary<Type, BindingFlags>();

        /// <summary>
        /// 缓存字段获取的flag cache,下次获取时不匹配的情况下重新获取
        /// </summary>
        private static Dictionary<Type, BindingFlags> propertyTypeFlagDict = new Dictionary<Type, BindingFlags>();


        public static Dictionary<string, FieldInfo> GetFields<T>(bool forceReFresh = false,BindingFlags flag = BindingFlags.Public| BindingFlags.Instance)
        {
            Type type = typeof(T);
            return GetFields(type, forceReFresh, flag);
        }

        public static Dictionary<string,FieldInfo> GetFields(Type type,bool forceReFresh = false,BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            AddFieldsToCache(type, forceReFresh, flag);
            return typeFieldDict[type];
        }

        public static FieldInfo GetField<T>(string fieldName,bool forceReFresh = false,BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            return GetField(typeof(T),fieldName,forceReFresh,flag);
        }

        public static FieldInfo GetField(Type type, string fieldName, bool forceReFresh = false, BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            AddFieldsToCache(type,forceReFresh,flag);
            var dict = typeFieldDict[type];
            if(dict.TryGetValue(fieldName,out FieldInfo info))
            {
                return info;
            }
            return null;
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="forceReFresh"></param>
        /// <param name="flag"></param>
        private static void AddFieldsToCache(Type type,bool forceReFresh,BindingFlags flag)
        {
            Dictionary<string, FieldInfo> fieldDict = null;
            if(forceReFresh || !typeFieldDict.TryGetValue(type,out fieldDict)|| fieldTypeFlagDict[type] != flag)
            {
                FieldInfo[] infos = type.GetFields(flag);
                if (fieldDict == null)
                    fieldDict = new Dictionary<string, FieldInfo>();
                else
                    fieldDict.Clear();

                foreach(var item in infos)
                {
                    fieldDict.Add(item.Name,item);
                }
                typeFieldDict[type] = fieldDict;
                fieldTypeFlagDict[type] = flag;
            }
        }

        public static Dictionary<string,PropertyInfo> GetProperties<T>(bool forceReFresh = false,BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            Type type = typeof(T);
            return GetProperties(type,forceReFresh,flag);
        }

        public static Dictionary<string,PropertyInfo> GetProperties(Type type,bool forceReFresh = false,BindingFlags flag = BindingFlags.Public| BindingFlags.Instance)
        {
            AddPropertiesToCache(type,forceReFresh,flag);
            return typePropertyDict[type];
        }

        public static PropertyInfo GetProperties<T>(string fieldName,bool forceReFresh = false,BindingFlags flag = BindingFlags.Public| BindingFlags.Instance)
        {
            return GetProperties(typeof(T),fieldName,forceReFresh,flag);
        }

        public static PropertyInfo GetProperties(Type type,string fieldName,bool forceReFresh = false,BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            AddPropertiesToCache(type,forceReFresh,flag);
            var dict = typePropertyDict[type];
            if(dict.TryGetValue(fieldName,out PropertyInfo info))
            {
                return info;
            }
            return null;
        }

        private static void AddPropertiesToCache(Type type,bool forceReFresh,BindingFlags flag)
        {
            Dictionary<string, PropertyInfo> propertyDict = null;
            if(forceReFresh || !typePropertyDict.TryGetValue(type,out propertyDict) || propertyTypeFlagDict[type]!= flag)
            {
                PropertyInfo[] infos = type.GetProperties(flag);
                if (propertyDict == null)
                    propertyDict = new Dictionary<string, PropertyInfo>();
                else
                    propertyDict.Clear();
                foreach(var item in infos)
                {
                    propertyDict.Add(item.Name,item);
                }
                typePropertyDict[type] = propertyDict;
                propertyTypeFlagDict[type] = flag;
            }
        }

        /// <summary>
        /// 释放缓存内容
        /// </summary>
        public static void Dispose()
        {
            typeFieldDict.Clear();
            fieldTypeFlagDict.Clear();
            typePropertyDict.Clear();
        }
    }

}