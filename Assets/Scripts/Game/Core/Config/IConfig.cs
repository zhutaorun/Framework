using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Interface
{
    public interface IConfig
    {
        void LoadStartTable();

        DSFramework.Function.Resource.ResourceManager.LoadAssetsHandle StartLoadAllTableAsync(bool reload = false, Action callback = null);

        void DisposeLoadAllTable(DSFramework.Function.Resource.ResourceManager.LoadAssetsHandle mainHandle);

#if LOAD_CONFIG_FROM_STREAMINGASSETS
        void StartLoadAllTableFromStreamingAssets();
#endif

        IEnumerator CoLoadTableAsync<T>(string assetPath) where T : class, IMessage;

        void Clear<T>();

        void ClearAll();

        T Get<T>(int nmetaID,bool log_error_if_null = true) where T:class,new();

        Dictionary<int, IMessage> GetAll<T>() where T : class, new();

        List<T> GetList<T>() where T : class;

        int GetCount<T>() where T : class, new();

        int GetCcount(Type type);

        Dictionary<int, IMessage>.Enumerator GetDataIter<T>();

        Dictionary<int, IMessage>.Enumerator GetDataIter(Type type);

    }
}