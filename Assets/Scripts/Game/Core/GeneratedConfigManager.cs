//
//    此文件自动生成，请不要手动修改，生成菜单：工具集（程序）/自动生成/生成配置表加载文件
//      Generate by sh-zhutao
//      data: 2023/2/26 22:44:20
//
using System;
using Google.Protobuf;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using static DSFramework.Function.Resource.ResourceManager;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace GameFrame.Config
{
  public partial class ConfigManager
      {
          private uint _loaderTimer;
          public LoadAssetsHandle StartLoadAllTableAsync(bool reload = false, Action callback = null)
          {
               List<AsyncOperationHandle> loaderHandleList = new List<AsyncOperationHandle>();
          LoadAssetsHandle mainHandle = new LoadAssetsHandle();
          mainHandle.allCount = loaderHandleList.Count;
          StartLoadTimer(loaderHandleList,mainHandle);
          callback?.Invoke();
          return mainHandle;
      }
          public IEnumerator CoLoadAllTable()
          {
            yield return null;
          }
#if LOAD_CONFIG_FROM_STREAMINGASSETS
          public void StartLoadAllTableFromStreamingAssets()
          {
          }
          void LoadFromStreamingAssets<T>(string assetPath) where T:class,IMessage
          {
              assetPath = Application.streamingAssetsPath+assetPath;
              UnityWebRequest request = new UnityWebRequest(assetPath);
              DownloadHandler downloadHandler = new DownloadHandlerBuffer();
              request.downloadHandler = downloadHandler
              request.SendWebRequset();
              while(!request.isDone){};
              if(!string.IsNullOrEmpty(request.error))
              {
                  Debug.LogError("load file failed,path:"+ assetPath);
                  return;
              }
              byte[] bytes = request.downloadHandler.data;
              MemorySteam ms = new MemorySteram(bytes);
              T data = System.Activator.CreateInstance<T>();
              data.MergeFrom(ms);
              OnLoadConfig(data);
          }
#endif
          public void DisposeLoadAllTable(LoadAssetsHandle mainHandle)
          {
              FrameTimer.DelTimer(_loaderTimer);
              mainHandle.Dispose();
          }
          private void StartLoadTimer(List<AsyncOperationHandle> loaderHandleList,LoadAssetsHandle mainHandle)
          {
              _loaderTimer = FrameTimer.AddTimer(0,1,()=>
              {
                  for (int i = 0;i < loaderHandleList.Count; i++)
                  {
                      var handle = loaderHandleList[i];
                      if(handle.IsDone)
                      {
                          mainHandle.loadedCount++;
                          mainHandle.progress = (float)mainHandle.loadedCount/(float)mainHandle.allCount;
                          loaderHandleList.Remove(handle);
                      }
                  }
              });
          }
          public void Add2LoadList<T>(string assetPath,List<AsyncOperationHandle> loaderHandleList) where T: class,IMessage
          {
              Debug.Log(assetPath);
              var handle = Hooks.ResourceManager.LoadAssetAsync<TextAsset>(assetPath,(asset)=>
              {
                  if(!asset)
                  {
                      Debug.LogError("load file failed,path:"+ assetPath);
                      return;
                  }
                  TextAsset textAsset = asset as TextAsset;
                  MemoryStream ms = new MemoryStream(textAsset.bytes);
                  T data = System.Activator.CreateInstance<T>();
                  data.MergeFrom(ms);
                  OnLoadConfig(data);
                  Debug.Log(assetPath +" load done");
              });
              loaderHandleList.Add(handle);
          }
          public IEnumerator CoLoadTableAsync<T>(string assetPath) where T :class,IMessage
          {
              TextAsset asset = null;
              yield return Hooks.ResourceManager.LoadAssetAsync<TextAsset>(assetPath,(textAsset)=>
              {
                  asset = textAsset;
              });
              if(!asset)
              {
                  Debug.LogError("load file failed,path:"+assetPath);
                  yield break;
              }
              MemoryStream ms = new MemoryStream(asset.bytes);
              T data = System.Activator.CreateInstance<T>();
              data.MergeFrom(ms);
              OnLoadConfig(data);
          }
          public void OnLoadConfig<T>(T data)
          {
              Type type = typeof(T);
        }
      }
}
