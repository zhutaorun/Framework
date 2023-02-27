using DSFramework.Function.Resource;
using GameFrame;
using GameFrame.Interface;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Config
{

    public partial class ConfigManager : ManagerBase, IConfig
    {
        public Dictionary<Type, Dictionary<int, IMessage>> DicTableContainer = new Dictionary<Type, Dictionary<int, IMessage>>();

        public override void Initialize()
        {
            base.Initialize();
        }

        public void LoadStartTable()
        {

        }

        public void Clear<T>()
        {
            Type key = typeof(T);
            if(DicTableContainer.ContainsKey(key))
            {
                DicTableContainer[key].Clear();
            }
        }

        public void ClearAll()
        {
            DicTableContainer.Clear();
        }

        #region Get
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nMetaID"></param>
        /// <param name="log_error_if_null">是否获取失败打印错误日志</param>
        /// <returns></returns>
        public T Get<T>(int nMetaID,bool log_error_if_null = true) where T:class,new ()
        {
            if (nMetaID <= 0) return null;

            Type key = typeof(T);
            if (!DicTableContainer.ContainsKey(key))
            {
                Debug.LogFormat(".Get {0}  table id {1} is null",key.FullName,nMetaID);
                return default(T);
            }

            Dictionary<int, IMessage> dictionary = DicTableContainer[key];
            if(!dictionary.ContainsKey(nMetaID))
            {
                if(log_error_if_null)
                {
                    Debug.LogFormat("Get {0} table id {1} is null",key.FullName,nMetaID);
                }
                return default(T);
            }
            return (dictionary[nMetaID] as T);
        }

        /// <summary>
        /// 获取所有配置Dict
        /// </summary>
        /// <typeparam name="T">目标配置表类型</typeparam>
        /// <returns></returns>
        public Dictionary<int,IMessage> GetAll<T>()where T: class,new()
        {
            Type key = typeof(T);
            if (!DicTableContainer.ContainsKey(key))
            {
                return null;
            }
            return DicTableContainer[key];
        }

        /// <summary>
        /// 获取所欲配置的List
        /// </summary>
        /// <typeparam name="T">目标配置表类型</typeparam>
        /// <returns></returns>
        public List<T> GetList<T>() where T :class
        {
            Type key = typeof(T);
            if(!this.DicTableContainer.ContainsKey(key))
            {
                return null;
            }
            List<T> listdata = new List<T>();
            Dictionary<int, IMessage>.Enumerator enumerator = DicTableContainer[key].GetEnumerator();
            while(enumerator.MoveNext())
            {
                listdata.Add(enumerator.Current.Value as T);
            }
            return listdata;
        }

        /// <summary>
        /// 获取配置表数量
        /// </summary>
        /// <typeparam name="T">目标配置表类型</typeparam>
        /// <returns></returns>
        public int GetCount<T>() where T:class,new()
        {
            Type key = typeof(T);
            Dictionary<int, IMessage> data = null;
            if(!DicTableContainer.TryGetValue(key,out data))
            {
                return 0;
            }
            return data.Count;
        }

        public Dictionary<int,IMessage>.Enumerator GetDataIter<T>()
        {
            Type key = typeof(T);
            if(!DicTableContainer.ContainsKey(key))
            {
                return new Dictionary<int, IMessage>().GetEnumerator();
            }
            return DicTableContainer[key].GetEnumerator();
        }

        public Dictionary<int,IMessage>.Enumerator GetDataIter(Type type)
        {
            if(!DicTableContainer.ContainsKey(type))
            {
                return new Dictionary<int, IMessage>().GetEnumerator();
            }
            return DicTableContainer[type].GetEnumerator();
        }

        ResourceManager.LoadAssetsHandle IConfig.StartLoadAllTableAsync(bool reload, Action callback)
        {
            throw new NotImplementedException();
        }

        void IConfig.DisposeLoadAllTable(ResourceManager.LoadAssetsHandle mainHandle)
        {
            throw new NotImplementedException();
        }

        IEnumerator IConfig.CoLoadTableAsync<T>(string assetPath)
        {
            throw new NotImplementedException();
        }

        int IConfig.GetCcount(Type type)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}