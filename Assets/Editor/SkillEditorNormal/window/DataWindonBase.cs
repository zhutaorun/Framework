using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class Data<T,U> where T: class,new() where U: class,new()
    {
        public Action<T> AddCallback;
        public Action LoadCallback;

        public List<object> _dataList = new List<object>();
        public List<T> _searchData = new List<T>();
        public T _select;
        protected string _input;
        protected string _configPath = "";
        protected string _idName;
        protected string _nameString;
        public string _dataName;
        private Type _typeT;
        private Type _typeU;
        private uint _maxID = 0;
        private U _container;
        private bool _reflectUseProperty;
        private MethodInfo _add;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configPath">配置路径</param>
        /// <param name="idString">id属性</param>
        /// <param name="nameString">名字属性</param>
        /// <param name="reflectUseProperty">是否使用getproperty获取名字跟id,区别于默认使用getfield获取</param>
        public Data(string configPath,string idString ,string nameString ,bool reflectUseProperty = true)
        {
            _configPath = configPath;
            _idName = idString;
            _nameString = nameString;
            _typeT = typeof(T);
            _typeU = typeof(U);
            _dataName = _typeT.Name;
            _reflectUseProperty = reflectUseProperty;
            if(_reflectUseProperty)
            {
                _add = _typeU.GetProperty("Infos").PropertyType.GetMethod("Add", new Type[] { typeof(int), _typeT });
            }
            else
            {
                //转移时结构不一样，此为旧结构
                _add = _typeU.GetMethod("Add",BindingFlags.Public | BindingFlags.Instance);
            }
        }


        public uint GetID(T data)
        {
            if(_reflectUseProperty)
            {
                PropertyInfo info = _typeT.GetProperty(_idName);
                if (info == null) return 0;
                object obj = info.GetValue(data);
                if (obj == null) return 0;
                if (info.PropertyType == typeof(uint))
                    return (uint)obj;
                else if (info.PropertyType == typeof(int))
                    return (uint)(int)obj;
                else
                    return 0;
            }
            else
            {
                FieldInfo info = _typeT.GetField(_idName);
                if (info == null) return 0;
                object obj = info.GetValue(data);
                if (obj == null) return 0;
                if (info.FieldType == typeof(uint))
                    return (uint)obj;
                else if (info.FieldType == typeof(int))
                    return (uint)(int)obj;
                else
                    return 0;
            }
        }

        public void SetID(T data,uint id)
        {
            if (_reflectUseProperty)
            {
                PropertyInfo info = _typeT.GetProperty(_idName);
                if (info == null) return ;
                if (info.PropertyType == typeof(uint))
                    info.SetValue(data, id);
                else if (info.PropertyType == typeof(int))
                    info.SetValue(data, (int)id);
            }
            else
            {
                FieldInfo info = _typeT.GetField(_idName);
                if (info == null) return;
                if (info.FieldType == typeof(uint))
                    info.SetValue(data,id);
                else if (info.FieldType == typeof(int))
                    info.SetValue(data, (int)id);
            }
        }

        public string GetName(T data)
        {
            if (_reflectUseProperty)
            {
                PropertyInfo info = _typeT.GetProperty(_nameString);
                if (info == null) return "";
                object obj = info.GetValue(data);
                if (obj == null) return "";
                return obj.ToString();
            }
            else
            {
                FieldInfo info = _typeT.GetField(_nameString);
                if (info == null) return "";
                object obj = info.GetValue(data);
                if (obj == null) return "";
                return obj.ToString();
            }
        }

        public void SetName(T data,string name)
        {
            if (_reflectUseProperty)
            {
                PropertyInfo info = _typeT.GetProperty(_nameString);
                if (info == null) return;
                info.SetValue(data, name);
            }
            else
            {
                FieldInfo info = _typeT.GetField(_nameString);
                if (info == null) return;
                info.SetValue(data,name);
            }
        }

        public void CollectData(string input)
        {
            _searchData.Clear();
            if (_dataList == null || _dataList.Count <= 0)
                return;
            
            foreach(var item in _dataList)
            {
                T data = item as T;
                if (data == null) return;
                if(GetID(data).ToString().Contains(input) || (GetName(data).Contains(input)))
                {
                    _searchData.Add(data);
                }
            }

            if (_searchData.Count > 0)
                _select = _searchData[0];
            if (LoadCallback != null)
                LoadCallback();
        }


        public void SaveConfig()
        {
            string bytePath = _configPath;

            _container = new U();
            if (_reflectUseProperty)
            {
                object infos = _typeU.GetProperty("Infos").GetValue(_container);
                if (infos != null)
                {
                    foreach (var item in _dataList)
                    {
                        _add.Invoke(infos, new object[] { (int)GetID(item as T), item as T });
                    }
                }
            }
            else
            {
                //转移时结构不一样，此为旧结构
                foreach (var item in _dataList)
                {
                    _add.Invoke(_container, new object[] { (int)GetID(item as T), item as T });
                }
            }
            SkillController.SkillCfg.SaveFile(bytePath, _container);
            AssetDatabase.Refresh();
        }


        public void SaveProtoBufferConfig()
        {

        }


        public uint AddConfig()
        {
            T data = EditorHook.configCtrl.CreateData<T>();
            uint id = ++_maxID;
            SetID(data,id);
            SetName(data,"未命名");
            if(_dataList == null)
            {
                _dataList = EditorHook.configCtrl.GetListData<T>();
            }
            AddCallback?.Invoke(data);
            return id;
        }


        public void DeleteConfig()
        {
            if (_select == null) return;

            _dataList.Remove(_select);
            CollectData("");
        }

        public void ReloadConfig(bool force = true)
        {
            if(!EditorHook.configCtrl.IsDataLoaded<T>() || force)
            {
                EditorHook.configCtrl.CreateData<T>();
                SkillController.SkillCfg.LoadConfig<T>("Assets/"+ _configPath);
            }

            _dataList = EditorHook.configCtrl.GetListData<T>();
            if(_dataList != null)
            {
                foreach(var item in _dataList)
                {
                    uint id = GetID(item as T);
                    if (id > _maxID)
                        _maxID = id;
                }
            }
            else
            {
                _maxID = 0;
            }
            CollectData(""); 
        }
    }

    public class DataWindowBase<T,U>:EditorWindow where T: class,new () where U: class,new()
    {
        protected Data<T, U> _data;
        protected string _input;
        private Vector2 _listScroll = Vector2.zero;
        protected Vector2 _attrScroll = Vector2.zero;

        public virtual void OnEnable()
        {
            minSize = new Vector2(400,200);
        }

        protected virtual void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(300));
                {
                    GUILayout.Space(10);
                    GUI_Head();
                    GUI_ListPart();
                }
                GUILayout.EndVertical();
                GUILayout.Space(20);
                GUI_AttrPart();
            }
            GUILayout.EndHorizontal();
        }


        private void GUI_Head()
        {
            EditorGUILayout.HelpBox("重新加载配置请点：重载配置按钮 ，或者重开Create Window 界面 ",MessageType.Info);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            string str = EditorGUILayout.TextField(_input,EditorStyles.toolbarSearchField);
            if(str != _input)
            {
                _input = str;
                OnInputChange();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                if(GUILayout.Button("保存"))
                {
                    _data.SaveConfig();
                }
                if(GUILayout.Button("添加"))
                {
                    uint id = _data.AddConfig();
                    _input = id.ToString();
                    OnInputChange();
                }
                if(GUILayout.Button("删除"))
                {
                    _data.DeleteConfig();
                }

                if(GUILayout.Button("重载配置"))
                {
                    _data.ReloadConfig();
                }
                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();
        }

        private void GUI_ListPart()
        {
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            {
                foreach(var item in _data._searchData)
                {
                    bool isCurrent = item == _data._select;
                    GUIStyleExtension.ApplySkinStyle(isCurrent ? GUIStyleExtension.defaultButtonSelectColor : default(Color), default(Color), default(Color));
                    if(GUILayout.Button(_data.GetID(item)+" " + _data.GetName(item)))
                    {
                        _data._select = item;
                    }
                    GUIStyleExtension.RecoverSkinStyle();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void GUI_AttrPart()
        {
            if (_data._select == null) return;

            _attrScroll = EditorGUILayout.BeginScrollView(_attrScroll);
            GUILayout.Space(10);
            AttrGUIHelper.GUI_Draw(_data._select,_data._dataName);
            EditorGUILayout.EndScrollView();
        }

        void OnInputChange()
        {
            _data.CollectData(_input);
        }
    }
}
