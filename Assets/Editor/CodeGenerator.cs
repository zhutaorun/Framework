using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.Build.Pipeline;
using JetBrains.Annotations;

public class CodeGenerator 
{
    const string AddressablePath = "Assets/AssetsPackgae";
    const string ConfigPath = "Config/TableData/bytes/";
    const string MenuName = EditorGlobal.Menu_Root_Program + "自动生成/生成配置表加载文件";
    [MenuItem(MenuName)]
    public static void GenerateConfigLoadScrip()
    {
        string filePaths = string.Format("{0}/{1}", AddressablePath, ConfigPath);
        string csPath = string.Format("{0}/Scripts/Game/Core/GeneratedConfigManager.cs",Application.dataPath);
        if(!Directory.Exists(filePaths))
        {
            Debug.LogError("路径不存在："+filePaths);
            return;
        }

        if(File.Exists(csPath))
        {
            File.Delete(csPath);
        }
        var csFile = File.Create(csPath);
        StreamWriter sw = new StreamWriter(csFile);
        try
        {
            List<string> names = new List<string>();
            string[] files = Directory.GetFiles(filePaths,"*.bytes");
            foreach(var file in files)
            {
                string name = Path.GetFileName(file);
                names.Add(name);
            }
            WriteData(sw,names);
        }
        catch(IOException e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (sw != null)
                sw.Dispose();
        }
        AssetDatabase.Refresh();
    }


    private static void WriteData(StreamWriter sw,List<string> names)
    {
        //写入说明与namespace
        WriteDescribtionAndNameSpace(sw);
        sw.WriteLine("namespace GameFrame.Config");
        sw.WriteLine("{");
        sw.WriteLine("  public partial class ConfigManager");
        sw.WriteLine("      {");
        //写入私有变量
        WritePrivateVariable(sw);
        //加载所有配置表
        WriteLoadAllTable(sw,names,true);
        WriteLoadAllTable(sw,names,false);
        sw.WriteLine("#if LOAD_CONFIG_FROM_STREAMINGASSETS");
        WriteLoadAllTableFromStreamingAssets(sw,names);
        LoadFromStreamingAssets(sw);
        sw.WriteLine("#endif");
        //关闭加载所有配置表
        WriteDisposeLoadAllTable(sw);
        //关闭加载配置表Timer
        WriteStartLoadTimer(sw);
        //添加到异步加载列表并开始加载
        WriteAdd2LoadList(sw);
        //加载单个配置表
        WriteCoLoadTableAsync(sw);
        //配置表加载回调
        WriteOnLoadConfig(sw,names);
        //添加到Dict
        sw.WriteLine("      }");
        sw.WriteLine("}");
    }

    /// <summary>
    /// 写入说明与namespace
    /// </summary>
    /// <param name="sw"></param>
    private static void WriteDescribtionAndNameSpace(StreamWriter sw)
    {
        sw.WriteLine("//");
        sw.WriteLine("//    此文件自动生成，请不要手动修改，生成菜单："+MenuName);
        sw.WriteLine(string.Format("//      Generate by {0}",SystemInfo.deviceName));
        sw.WriteLine(string.Format("//      data: {0}",System.DateTime.Now));
        sw.WriteLine("//");
        sw.WriteLine("using System;");
        sw.WriteLine("using Google.Protobuf;");
        sw.WriteLine("using System.IO;");
        sw.WriteLine("using UnityEngine;");
        sw.WriteLine("using System.Collections;");
        sw.WriteLine("using System.Collections.Generic;");
        sw.WriteLine("using UnityEngine.Networking;");
        sw.WriteLine("using static DSFramework.Function.Resource.ResourceManager;");
        sw.WriteLine("using UnityEngine.ResourceManagement.AsyncOperations;");
    }

    /// <summary>
    /// 写入私有变量
    /// </summary>
    /// <param name="sw"></param>
    private static void WritePrivateVariable(StreamWriter sw)
    {
        sw.WriteLine("          private uint _loaderTimer;");
    }

    private static void WriteLoadAllTable(StreamWriter sw,List<string> names,bool asyncLoad)
    {
        if(asyncLoad)
        {
            sw.WriteLine("          public LoadAssetsHandle StartLoadAllTableAsync(bool reload = false, Action callback = null)");
            sw.WriteLine("          {");
            sw.WriteLine("               List<AsyncOperationHandle> loaderHandleList = new List<AsyncOperationHandle>();");
        }
        else
        {
            sw.WriteLine("          public IEnumerator CoLoadAllTable()");
            sw.WriteLine("          {");
        }

        for(int i=0;i<names.Count;i++)
        {
            if (AnalyseConfigName(names[i],out string dateType,out string containerType))
            {
                if(asyncLoad)
                {
                    sw.WriteLine(string.Format("            Add2LoadList<{0}>(\"{1}\",loaderHandleList);", containerType, AddressablePath + ConfigPath + names[i]));
                }
                else
                {
                    sw.WriteLine(string.Format("            yield return CoLoadTableAsync<{0}>(\"{1}\",loaderHandleList);", containerType, AddressablePath + ConfigPath + names[i]));
                }
            }
        }

        if(asyncLoad)
        {
            sw.WriteLine("          LoadAssetsHandle mainHandle = new LoadAssetsHandle();");
            sw.WriteLine("          mainHandle.allCount = loaderHandleList.Count;");
            sw.WriteLine("          StartLoadTimer(loaderHandleList,mainHandle);");
            sw.WriteLine("          callback?.Invoke();");
            sw.WriteLine("          return mainHandle;");
        }
        sw.WriteLine("             }");
    }

    private static void WriteDisposeLoadAllTable(StreamWriter sw)
    {
        sw.WriteLine("          public void DisposeLoadAllTable(LoadAssetsHandle mainHandle)");
        sw.WriteLine("          {");
        sw.WriteLine("              FrameTimer.DelTimer(_loaderTimer);");
        sw.WriteLine("              mainHandle.Dispose();");
        sw.WriteLine("          }");
    }

    private static void WriteStartLoadTimer(StreamWriter sw)
    {
        sw.WriteLine("          private void StartLoadTimer(List<AsyncOperationHandle> loaderHandleList,LoadAssetsHandle mainHandle)");
        sw.WriteLine("          {");
        sw.WriteLine("              _loaderTimer = FrameTimer.AddTimer(0,1,()=>");
        sw.WriteLine("              {");
        sw.WriteLine("                  for (int i = 0;i < loaderHandleList.Count; i++)");
        sw.WriteLine("                  {");
        sw.WriteLine("                      var handle = loaderHandleList[i];");
        sw.WriteLine("                      if(handle.IsDone)");
        sw.WriteLine("                      {");
        sw.WriteLine("                          mainHandle.loadedCount++;");
        sw.WriteLine("                          mainHandle.progress = (float)mainHandle.loadedCount/(float)mainHandle.allCount;");
        sw.WriteLine("                          loaderHandleList.Remove(handle);");
        sw.WriteLine("                      }");
        sw.WriteLine("                  }");
        sw.WriteLine("              });");
        sw.WriteLine("          }");
    }


    private static void WriteAdd2LoadList(StreamWriter sw)
    {
        sw.WriteLine("          public void Add2LoadList<T>(string assetPath,List<AsyncOperationHandle> loaderHandleList) where T: class,IMessage");
        sw.WriteLine("          {");
        sw.WriteLine("              Debug.Log(assetPath);");
        sw.WriteLine("              var handle = Hooks.ResourceManager.LoadAssetAsync<TextAsset>(assetPath,(asset)=>");
        sw.WriteLine("              {");
        sw.WriteLine("                  if(!asset)");
        sw.WriteLine("                  {");
        sw.WriteLine("                      Debug.LogError(\"load file failed,path:\"+ assetPath);");
        sw.WriteLine("                      return;");
        sw.WriteLine("                  }");
        sw.WriteLine("                  TextAsset textAsset = asset as TextAsset;");
        sw.WriteLine("                  MemoryStream ms = new MemoryStream(textAsset.bytes);");
        sw.WriteLine("                  T data = System.Activator.CreateInstance<T>();");
        sw.WriteLine("                  data.MergeFrom(ms);");
        sw.WriteLine("                  OnLoadConfig(data);");
        sw.WriteLine("                  Debug.Log(assetPath +\" load done\");");
        sw.WriteLine("              });");
        sw.WriteLine("              loaderHandleList.Add(handle);");
        sw.WriteLine("          }");
    }


    /// <summary>
    /// StreamingAssets加载所有配置表
    /// </summary>
    /// <param name="sw"></param>
    /// <param name="names"></param>
    private static void WriteLoadAllTableFromStreamingAssets(StreamWriter sw,List<string> names)
    {
        sw.WriteLine("          public void StartLoadAllTableFromStreamingAssets()");
        sw.WriteLine("          {");

        for(int i=0;i<names.Count;i++)
        {
            if (AnalyseConfigName(names[i],out string dataType,out string coutainerType))
            {
                sw.WriteLine(string.Format("            LoadFromStreamingAssets<{0}>(\"{1}\");", coutainerType, "/" + ConfigPath + names[i]));
            }
        }
        sw.WriteLine("          }");
    }

    private static void LoadFromStreamingAssets(StreamWriter sw)
    {
        sw.WriteLine("          void LoadFromStreamingAssets<T>(string assetPath) where T:class,IMessage");
        sw.WriteLine("          {");
        sw.WriteLine("              assetPath = Application.streamingAssetsPath+assetPath;");
        sw.WriteLine("              UnityWebRequest request = new UnityWebRequest(assetPath);");
        sw.WriteLine("              DownloadHandler downloadHandler = new DownloadHandlerBuffer();");
        sw.WriteLine("              request.downloadHandler = downloadHandler");
        sw.WriteLine("              request.SendWebRequset();");
        sw.WriteLine("              while(!request.isDone){};");
        sw.WriteLine("              if(!string.IsNullOrEmpty(request.error))");
        sw.WriteLine("              {");
        sw.WriteLine("                  Debug.LogError(\"load file failed,path:\"+ assetPath);");
        sw.WriteLine("                  return;");
        sw.WriteLine("              }");
        sw.WriteLine("              byte[] bytes = request.downloadHandler.data;");
        sw.WriteLine("              MemorySteam ms = new MemorySteram(bytes);");
        sw.WriteLine("              T data = System.Activator.CreateInstance<T>();");
        sw.WriteLine("              data.MergeFrom(ms);");
        sw.WriteLine("              OnLoadConfig(data);");
        sw.WriteLine("          }");
    }


    /// <summary>
    /// 加载单个配置表：只提供异步加载，如需要同步协程模拟
    /// </summary>
    /// <param name="sw"></param>
    private static void WriteCoLoadTableAsync(StreamWriter sw)
    {
        sw.WriteLine("          public IEnumerator CoLoadTableAsync<T>(string assetPath) where T :class,IMessage");
        sw.WriteLine("          {");
        sw.WriteLine("              TextAsset asset = null;");
        sw.WriteLine("              yield return Hooks.ResourceManager.LoadAssetAsync<TextAsset>(assetPath,(textAsset)=>");
        sw.WriteLine("              {");
        sw.WriteLine("                  asset = textAsset;");
        sw.WriteLine("              });");
        sw.WriteLine("              if(!asset)");
        sw.WriteLine("              {");
        sw.WriteLine("                  Debug.LogError(\"load file failed,path:\"+assetPath);");
        sw.WriteLine("                  yield break;");
        sw.WriteLine("              }");
        sw.WriteLine("              MemoryStream ms = new MemoryStream(asset.bytes);");
        sw.WriteLine("              T data = System.Activator.CreateInstance<T>();");
        sw.WriteLine("              data.MergeFrom(ms);");
        sw.WriteLine("              OnLoadConfig(data);");
        sw.WriteLine("          }");
    }

    private static void WriteOnLoadConfig(StreamWriter sw,List<string> names)
    {
        sw.WriteLine("          public void OnLoadConfig<T>(T data)");
        sw.WriteLine("          {");
        sw.WriteLine("              Type type = typeof(T);");

        bool firstOne = true;
        List<string> writeType = new List<string>();
        for(int i=0;i<names.Count;i++)
        {
            if (AnalyseConfigName(names[i], out string dataType, out string containerType))
            {
                if(writeType.Contains(dataType))
                {
                    continue;
                }
                writeType.Add(dataType);
                if(firstOne)
                {
                    firstOne = false;
                    sw.WriteLine(string.Format("            if(type == typeof({0}))", containerType));
                }
                else
                {
                    sw.WriteLine(string.Format("            else if(type == typeof({0}))", containerType));
                }
                sw.WriteLine("              {");
                sw.WriteLine(string.Format("                var dict = (data as {0}.Infos;)",containerType));
                sw.WriteLine(string.Format("                AddDicTable<{0}>(dict);",dataType)) ;
                sw.WriteLine("              }");
            }
        }
        sw.WriteLine("        }");
    }

    /// <summary>
    /// 添加到Dict
    /// </summary>
    /// <param name="sw"></param>
    private static void WriteAddDictTable(StreamWriter sw)
    {
        sw.WriteLine("          private void AddDitcTable<T>(Google.Protobuf.Collections.MapField<int,T> dict) where T : IMessage");
        sw.WriteLine("          {");
        sw.WriteLine("              Type dataType = typeof(T);");
        sw.WriteLine("              Dictionary<int,IMessage> configDict = null;");
        sw.WriteLine("              {");
        sw.WriteLine("                  configDict = new Dictionary<int,IMessage>();");
        sw.WriteLine("                  DicTableContainer.Add(dataType,configDict);");
        sw.WriteLine("              }");
        sw.WriteLine("              foreach(var item in dict)");
        sw.WriteLine("              {");
        sw.WriteLine("                  if(!configDict.ContainsKey(item.Key))");
        sw.WriteLine("                  {");
        sw.WriteLine("                      configDict.Add(item.Key,item.Value);");
        sw.WriteLine("                  }");
        sw.WriteLine("                  else");
        sw.WriteLine("                  {");
        sw.WriteLine("                      Debug.LorError($\"配置表{dataType.Name} 存在相同的key:{item.Key.ToString()}\"});");
        sw.WriteLine("                  }");
        sw.WriteLine("              }");
        sw.WriteLine("          }");
    }

    private static bool AnalyseConfigName(string name,out string dataType,out string containerType)
    {
        dataType = string.Empty;
        containerType = string.Empty;

        if (!name.Contains(".")) return false;
        name = name.Split('.')[0];
        if (!name.Contains("@")) return false;

        name = name.Split('@')[1];
        string spacename = get_meta_name_2_type_name(name);
        string space = string.Format("{0}Config",spacename);
        dataType = string.Format("{0}.{1}_data",space,name);
        containerType = string.Format("{0}.contaniner", space);
        return true;
    }

    static string get_meta_name_2_type_name(string filename)
    {
        string _fileName = "";
        for(int i=0;i<filename.Length;i++)
        {
            char f = filename[i];
            if(i==0)
            {
                _fileName += (f.ToString().ToUpper());//首字母大写
            }
            else
            {
                if(f.Equals('_'))
                {
                    if(i+1<filename.Length)
                    {
                        char nextF = filename[i + 1];
                        if(!nextF.Equals('_'))
                        {
                            _fileName += (nextF.ToString().ToUpper());//遇到下一个划线后下一个字母大写
                            i++;
                        }
                    }
                }
                else
                {
                    _fileName += f;
                }
            }
        }
        return _fileName;
    }
}
