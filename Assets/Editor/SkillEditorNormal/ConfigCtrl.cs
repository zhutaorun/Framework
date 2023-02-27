using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using System.Reflection;

namespace GameEditor
{
    public partial class ConfigCtrl : IDisposable
    {
        private Dictionary<string, List<object>> m_data = new Dictionary<string, List<object>>();//配置数据集合(List)使用过程中需要修改id的原因不使用dict

        private bool m_AllSkillDataLoaded = false;//是否所有技能加载完毕，使用全部加载才会设置
        private List<int> m_LoadedUnitSkill = new List<int>();//已加载的unit相关技能

        public void Dispose()
        {
            m_AllSkillDataLoaded = false;
            ClearAllConfig();
            m_LoadedUnitSkill.Clear();
        }

        /// <summary>
        /// 加载所有需要的配置表，技能除外
        /// </summary>
        public void LoadStartConfig()
        {
            SkillController.SkillCfg.LoadConfig<UnitConfig.unit_data>("Assets/"+SkillDefine.UnitConfigPath);
            SkillController.SkillCfg.LoadConfig<EffectConfig.effect_data>("Assets/" + SkillDefine.EffectConfigPath);
            //SkillController.SkillCfg.LoadConfig<BulletConfig.bullet_data>("Assets/" + SkillDefine.BulletConfigPath);
            //SkillController.SkillCfg.LoadConfig<BuffConfig.buff_data>("Assets/" + SkillDefine.BuffConfigPath);
            SkillController.SkillCfg.LoadConfig<SkillnewConfig.skillnew_data>("Assets/" + SkillDefine.SkillConfigPath);
            //SkillController.SkillCfg.LoadConfig<ShakeConfig.shake_data>("Assets/" + SkillDefine.ShakeConfigPath);
        }

        public void LoadSkillConfig(int unit_id,bool loadSeprateSkill = false)
        {
            if (unit_id > 0 && m_LoadedUnitSkill.Contains(unit_id)) return;
            if(!loadSeprateSkill)
            {
                SkillController.SkillCfg.LoadConfig<SkillnewConfig.skillnew_data>("Assets/"+SkillDefine.SkillConfigPath);
                m_AllSkillDataLoaded = true;
                return;
            }

            try
            {
                string Path = "";
                if(unit_id>0)
                {
                    Path = string.Format("{0}/{1}/{2}",Application.dataPath,SkillDefine.SeparatedSkillFolder,unit_id);
                }
                else
                {
                    Path = string.Format("{0}/{1}",Application.dataPath,SkillDefine.SeparatedSkillFolder);
                }

                if(!Directory.Exists(Path))
                {
                    Debug.LogError("directory not exist:"+Path);
                    return;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(Path);
                FileInfo[] files = directoryInfo.GetFiles("*.bytes",SearchOption.AllDirectories);

                for(int i=0;i<files.Length;i++)
                {
                    if (!files[i].Name.EndsWith("bytes"))
                        continue;
                    EditorUtility.DisplayProgressBar("提示",files[i] +"加载中...",(float)i/files.Length);

                    string assetPath = unit_id > 0 ?
                        string.Format("Assets/{0}/{1}/{2}", SkillDefine.SeparatedSkillFolder, unit_id, files[i].Name) :
                        files[i].FullName.Replace(System.IO.Path.GetFullPath(Application.dataPath),"Assets/");

                    SkillController.SkillCfg.LoadSeparatedConfig<SkillnewConfig.skillnew_data>(assetPath);
                }
                EditorUtility.ClearProgressBar();
            }
            catch(IOException e)
            {
                Debug.LogError(e.ToString());
            }

            if(unit_id>0)
            {
                m_LoadedUnitSkill.Add(unit_id);
            }
            else
            {
                m_AllSkillDataLoaded = true;
            }
        }

        public void CheckLoadSkillConfig(int unit_id = 0)
        {
            LoadSkillConfig(unit_id);
        }

        /// <summary>
        /// 检查id是否重复
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="idName"></param>
        /// <returns></returns>
        public bool CheckIDRepeated<T>(int id,string idName = "id") where T: class,new()
        {
            List<object> list = EditorHook.configCtrl.GetListData<T>();
            if (list == null) return false;
            int count = 0;

            foreach(var item  in list)
            {
                Type type = typeof(T);
                PropertyInfo info = type.GetProperty(idName);
                int getid = (int)info.GetValue(item);
                if (getid == id)
                    count++;

                if (count >= 2)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="idName"></param>
        /// <returns></returns>
        public bool ContainsID<T>(int id,string idName ="id") where T: class,new ()
        {
            List<object> list = EditorHook.configCtrl.GetListData<T>();
            if (list == null) return false;
            int count = 0;

            foreach (var item in list)
            {
                Type type = typeof(T);
                PropertyInfo info = type.GetProperty(idName);
                int getid = (int)info.GetValue(item);
                if (getid == id)
                    return true;
            }
            return false;
        }

        #region 数据
        /// <summary>
        /// 反射获取配置id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="idname"></param>
        /// <returns></returns>
        public int GetID<T>(T data,string idname ="Id") where T:class,new()
        {
            FieldInfo field = typeof(T).GetField(idname);
            if(field== null)
            {
                Debug.LogError("尝试获取错误的属性，属性名:"+idname);
                return 0;
            }
            bool isuint = field.FieldType == typeof(uint);
            int fieldvalue = isuint ? (int)(uint)field.GetValue(data) : (int)field.GetValue(data);
            return fieldvalue;
        }

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="idname"></param>
        /// <returns></returns>
        public T GetData<T>(int id,string idname = "Id")where T:class,new()
        {
            GetListData<T>();
            List<object> result = null;
            PropertyInfo field = typeof(T).GetProperty(idname);
            bool isuint = field.PropertyType == typeof(uint);
            if(m_data.TryGetValue(typeof(T).Name,out result))
            {
                foreach(var item in result)
                {
                    int fieldvalue = isuint ? (int)(uint)field.GetValue(item) : (int)field.GetValue(item);
                    if(fieldvalue == id)
                    {
                        return item as T;
                    }
                }
            }
            return null;
        }

        public List<object> GetListData<T>() where T :class,new()
        {
            List<object> result = null;
            string name = typeof(T).Name;
            bool exist = m_data.TryGetValue(name, out result);
            Dictionary<int, object> data = SkillController.SkillCfg.GetAll<T>();

            if(!exist || result == null || result.Count!= data.Count)
            {
                if (data == null) return null;
                result = new List<object>();

                foreach(var item in data)
                {
                    result.Add(item.Value);
                }

                m_data[name] = result;
            }

            return result;
        }

        public T CreateData<T>() where T:class,new()
        {
            List<object> result = null;
            if(!m_data.TryGetValue(typeof(T).Name,out result))
            {
                Dictionary<int, object> data = SkillController.SkillCfg.GetAll<T>();
                result = new List<object>();
                if(data != null)
                {
                    foreach(var item in data)
                    {
                        result.Add(item.Value);
                    }
                }
                m_data.Add(typeof(T).Name, result);
            }
            T t = new T();
            result.Add(t);
            return t;
        }

        /// <summary>
        /// 更新配置信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idName"></param>
        public void FreshConfigData<T>(string idName ="id")where T: class,new()
        {
            SkillController.SkillCfg.Clear<T>();
            List<object> result = null;
            if(!m_data.TryGetValue(typeof(T).Name,out result))
            {
                return;
            }

            foreach(var item in result)
            {
                int id = GetID(item as T, idName);
                SkillController.SkillCfg.AddConfig<T>(item as T, id);
            }
        }

        /// <summary>
        /// 清除一类配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ClearData<T>()
        {
            m_data.Remove(typeof(T).Name);
            SkillController.SkillCfg.Clear<T>();
        }

        public bool IsDataLoaded<T>()
        {
            return m_data.ContainsKey(typeof(T).Name);
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void DeleteData<T>(T data) where T:class,new()
        {
            List<object> result = null;
            if(m_data.TryGetValue(typeof(T).Name,out result))
            {
                foreach(var item in result)
                {
                    if(item == data)
                    {
                        result.Remove(item);
                        break;
                    }
                }
            }
        }

        public void ClearAllConfig()
        {
            SkillController.SkillCfg.ClearAll();
            m_data.Clear();
        }

        #endregion
    }

}