using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using System.Reflection;
using Google.Protobuf.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditorInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFrame
{ 
    public class SkillConfigManager
    {
        #region Variables 
        public Dictionary<Type, Dictionary<int, object>> DicTableContainer = new Dictionary<Type, Dictionary<int, object>>();
        #endregion

        public void Initialize()
        {
            DicTableContainer.Clear();
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Dictionary<int,object> GetAll<T>() where  T:class,new()
        {
            if(Application.isPlaying)
            {
                return null;
            }
            else
            {
                Type t = typeof(T);
                if(DicTableContainer.ContainsKey(t) == false)
                {
                    return null;
                }
                return DicTableContainer[t];
            }
        }

        /// <summary>
        /// 获取单个配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(int id) where T : class,new()
        {
            if(Application.isPlaying)
            {
                return Hooks.ConfigManager.Get<T>(id);
            }
            else
            {
                Type t = typeof(T);
                if(DicTableContainer.TryGetValue(t,out Dictionary<int,object> dict))
                {
                    if (dict.ContainsKey(id))
                        return dict[id] as T;
                    return null;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取配置list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetList<T>() where T:class,new()
        {
            Type t = typeof(T);
            if(DicTableContainer.TryGetValue(t,out Dictionary<int,object> dict))
            {
                if (dict.Count < 0)
                    return new List<T>();
                List<T> list = new List<T>();
                foreach(var item in dict)
                {
                    list.Add(item.Value as T);
                }
                return list;
            }
            return new List<T>();
        }
        /// <summary>
        /// 加载配置表，替换掉已存在的类型dict数据(list)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="serilizeByProtoBuffer"></param>
        public void LoadConfig<T>(string name,bool serilizeByProtoBuffer = true) where T : class,new()
        {
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(name);
            if(asset == null)
            {
                DicTableContainer[typeof(T)] = new Dictionary<int, object>();
                return;
            }
            MemoryStream ms = new MemoryStream(asset.bytes);
            Type type = typeof(T);
            if(serilizeByProtoBuffer)
            {
                string containerTypeName = type.Namespace + ".container";
                Type containerType = Type.GetType(containerTypeName);
                if(containerType == null)
                {
                    Debug.LogError("获取container类型错误(not found):"+containerTypeName);
                }
                var data = Activator.CreateInstance(containerType) as IMessage;
                data.MergeFrom(ms);
                PropertyInfo info = containerType.GetProperty("Infos");
                if(info == null)
                {
                    Debug.LogError("属性错误，目标类型没有 infos Property,type:"+ typeof(T));
                    return;
                }
                var value = info.GetValue(data);
                var infoDict = (MapField<int,T>)value;
                Dictionary<int, object> objDict = ToObjectDict(infoDict);
                DicTableContainer[type] = objDict;
            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                SkillConfig.ConfigContainer<int, T> data = bf.Deserialize(ms) as SkillConfig.ConfigContainer<int, T>;
                Dictionary<int, object> dict = data.ToObjectDict();
                DicTableContainer[type] = dict;
            }
 #else
            Hooks.ResourceManager.LoadAssetAsync<TextAsset>(name, (textAsset) => 
            {
                TextAsset asset = textAsset;
                if(!asset)
                {
                    Debug.LogError("load file failed,path:"+name);
                    return;
                }
                MemoryStream ms = new MemoryStream(asset.bytes);
                Type type = typeof(T);
                if(serilizeByProtoBuffer)
                {
                    string containerTypeName = type.Namespace + ".container";
                    Type containerType = Type.GetType(containerTypeName);
                    if (containerType == null)
                    {
                        Debug.LogError("获取container类型错误(not found):" + containerTypeName);
                    }
                    var data = Activator.CreateInstance(containerType) as IMessage;
                    data.MergeFrom(ms);
                    PropertyInfo info = containerType.GetProperty("Infos");
                    if (info == null)
                    {
                        Debug.LogError("属性错误，目标类型没有 infos Property,type:" + typeof(T));
                        return;
                    }
                    var value = info.GetValue(data);
                    var infoDict = (MapField<int, T>)value;
                    Dictionary<int, object> objDict = ToObjectDict(infoDict);
                    DicTableContainer[type] = objDict;
                }
                else
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    SkillConfig.ConfigContainer<int, T> data = bf.Deserialize(ms) as SkillConfig.ConfigContainer<int, T>;
                    Dictionary<int, object> dict = data.ToObjectDict();
                    DicTableContainer[type] = dict;
                }
            });
#endif
        }

        /// <summary>
        /// 加载多文件数据，保留已存在的类型dict数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="serilizeByProtoBuffer"></param>
        public void LoadSeparatedConfig<T>(string name,bool serilizeByProtoBuffer = true) where T:class,new()
        {
#if UNITY_EDITOR
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(name);
            if (!asset)
            {
                Debug.LogError("load file failed,path:"+name); 
                return;
            }
            MemoryStream ms = new MemoryStream(asset.bytes);
            Type type = typeof(T);
            if (serilizeByProtoBuffer)
            {
                string containerTypeName = type.Namespace + ".container";
                Type containerType = Type.GetType(containerTypeName);
                if (containerType == null)
                {
                    Debug.LogError("获取container类型错误(not found):" + containerTypeName);
                }
                var data = Activator.CreateInstance(containerType) as IMessage;
                data.MergeFrom(ms);
                PropertyInfo info = containerType.GetProperty("Infos");
                if (info == null)
                {
                    Debug.LogError("属性错误，目标类型没有 infos Property,type:" + typeof(T));
                    return;
                }
                var infoDict = (MapField<int, T>)info.GetValue(data);
                foreach(var item in infoDict)
                {
                    if(!DicTableContainer.ContainsKey(type))
                    {
                        DicTableContainer[type] = new Dictionary<int, object>();
                    }
                    DicTableContainer[type][item.Key] = item.Value;
                }
            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                SkillConfig.ConfigContainer<int, T> data = bf.Deserialize(ms) as SkillConfig.ConfigContainer<int, T>;
                Dictionary<int, object> dict = data.ToObjectDict();
                foreach (var item in dict)
                {
                    if (!DicTableContainer.ContainsKey(type))
                    {
                        DicTableContainer[type] = new Dictionary<int, object>();
                    }
                    DicTableContainer[type][item.Key] = item.Value;
                }
            }
#else
            Hooks.ResourceManager.LoadAssetAsync<TextAsset>(name, (textAsset) => 
            {
                TextAsset asset = textAsset;
                if(!asset)
                {
                    Debug.LogError("load file failed,path:"+name);
                    return;
                }
                MemoryStream ms = new MemoryStream(asset.bytes);
                Type type = typeof(T);
                if(serilizeByProtoBuffer)
                {
                    string containerTypeName = type.Namespace + ".container";
                    Type containerType = Type.GetType(containerTypeName);
                    if (containerType == null)
                    {
                        Debug.LogError("获取container类型错误(not found):" + containerTypeName);
                    }
                    var data = Activator.CreateInstance(containerType) as IMessage;
                    data.MergeFrom(ms);
                    PropertyInfo info = containerType.GetProperty("Infos");
                    if (info == null)
                    {
                        Debug.LogError("属性错误，目标类型没有 infos Property,type:" + typeof(T));
                        return;
                    }
                    var infoDict = (MapField<int, T>)info.GetValue(data);
                    foreach(var item in infoDict)
                    {
                        if(!DicTableContainer.ContainsKey(type))
                        {
                            DicTableContainer[type] = new Dictionary<int, object>();
                        }
                        DicTableContainer[type][item.Key] = item.Value;
                    }
                }
                else
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    SkillConfig.ConfigContainer<int, T> data = bf.Deserialize(ms) as SkillConfig.ConfigContainer<int, T>;
                    Dictionary<int, object> dict = data.ToObjectDict();
                    foreach (var item in dict)
                    {
                        if (!DicTableContainer.ContainsKey(type))
                        {
                            DicTableContainer[type] = new Dictionary<int, object>();
                        }
                        DicTableContainer[type][item.Key] = item.Value;
                    }
                }
            });
#endif
        }

        public void SaveFile<T>(string path,T data,bool serilizeByProtoBuffer = true) where T: class,new()
        {
#if UNITY_EDITOR
            path = Application.dataPath + "/" + path;
            MemoryStream ms = new MemoryStream();
            if(serilizeByProtoBuffer)
            {
                (data as IMessage).WriteTo(ms);
            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms,data);
            }
            ms.Seek(0,0);
            FileStream fs = null;
            try 
            {
                string folderPath = path.Substring(0, path.LastIndexOf("/"));
                if(!Directory.Exists(folderPath))
                {
                    if(EditorUtility.DisplayDialog("Tips",string.Format("目标文件夹{0}不存在，是否创建",folderPath),"创建","取消"))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    else
                    {
                        return;
                    }
                }

                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                fs = File.Open(path,FileMode.OpenOrCreate,FileAccess.Write);
                ms.WriteTo(fs);
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
            finally
            {
                if(fs != null)
                {
                    fs.Close();
                }
                ms.Close();
            }
#endif
        }

        private Type GetTableType(string strtableType)
        {
            Type t = Type.GetType(strtableType);
            if(t != null)
            {
                return t;
            }

            Assembly[] assembly = AppDomain.CurrentDomain.GetAssemblies();
            for(int i=0;i<assembly.Length;i++)
            {
                t = assembly[i].GetType(strtableType);
                if(t != null)
                {
                    break;
                }
            }
            return t;
        }

        /// <summary>
        /// 清理指定类型的配置数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>()
        {
            Type t = typeof(T);
            if(DicTableContainer.ContainsKey(t))
            {
                DicTableContainer[t].Clear();
            }    
        }

        /// <summary>
        /// 全部清理
        /// </summary>
        public void ClearAll()
        {
            DicTableContainer.Clear();
        }

        public void AddConfig<T>(T data,int key)
        {
#if UNITY_EDITOR
            Dictionary<int, object> dict = null;
            if(!DicTableContainer.TryGetValue(typeof(T),out dict))
            {
                dict = new Dictionary<int, object>();
                DicTableContainer[typeof(T)] = dict;
            }
            dict[key] = data;
#endif
        }

        private Dictionary<int,object> ToObjectDict<T>(MapField<int,T> data)
        {
            Dictionary<int, object> result = new Dictionary<int, object>();
            foreach(var item in data)
            {
                result.Add(item.Key,item.Value);
            }
            return result;
        }
    }
}