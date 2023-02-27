using GameFrame;
using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public static partial class AttrGUIHelper 
    {
        /// <summary>
        /// true:反射绘制field属性，false:反射绘制property属性
        /// </summary>
        private static bool ReflectFieldMode = false;

        /// <summary>
        /// GUI_List函数信息
        /// </summary>
        private static MethodInfo m_GUI_List_Method;

        private static MethodInfo GUI_List_Method
        {
            get
            {
                if (m_GUI_List_Method == null)
                {
                    m_GUI_List_Method = typeof(AttrGUIHelper).GetMethod("GUI_List");
                }
                return m_GUI_List_Method;
            }
        }


        /// <summary>
        /// GUI_RepeatedField函数信息
        /// </summary>
        private static MethodInfo m_GUI_RepeatedField_Method;

        private static MethodInfo GUI_RepeatedField_Method
        {
            get
            {
                if (m_GUI_RepeatedField_Method == null)
                {
                    m_GUI_RepeatedField_Method = typeof(AttrGUIHelper).GetMethod("GUI_RepeatedField");
                }
                return m_GUI_RepeatedField_Method;
            }
        }

        /// <summary>
        /// GUI_Array函数信息
        /// </summary>
        private static MethodInfo m_GUI_Array_Method;

        private static MethodInfo GUI_Array_Method
        {
            get
            {
                if (m_GUI_Array_Method == null)
                {
                    m_GUI_Array_Method = typeof(AttrGUIHelper).GetMethod("GUI_Array");
                }
                return m_GUI_Array_Method;
            }
        }

        /// <summary>
        /// GUI_Dict函数信息
        /// </summary>
        private static MethodInfo m_GUI_Dict_Method;

        private static MethodInfo GUI_Dict_Method
        {
            get
            {
                if (m_GUI_Dict_Method == null)
                {
                    m_GUI_Dict_Method = typeof(AttrGUIHelper).GetMethod("GUI_Dict");
                }
                return m_GUI_Dict_Method;
            }
        }

        /// <summary>
        /// 反射显示所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="space"></param>
        public static void GUI_Draw<T>(T data,string name,int space=0)
        {
            Type type = typeof(T);
            object result = GUI_Draw(type,data,name,space);
        }


        public static object GUI_Draw(Type type, object data, string name, int space = 0)
        {
            if (CheckAttributes(type, ref data, name, space))
            {
                return data;
            }

            //基础类型
            if (type.IsValueType || type == TypeString)
            {
                if (type == TypeBool)
                {
                    data = GUI_Bool_Attr(name, (bool)data, space);
                }
                else if (type == TypeInt)
                {
                    data = GUI_Int_Attr(name, (int)data, space);
                }
                else if (type == TypeUInt)
                {
                    data = GUI_UInt_Attr(name, (uint)data, space);
                }
                else if (type == TypeFloat)
                {
                    data = GUI_Float_Attr(name, (float)data, space);
                }
                else if (type == TypeUShort)
                {
                    data = (ushort)GUI_UInt_Attr(name, (ushort)data, space);
                }
                else if (type == TypeString)
                {
                    if (data == null) data = "";
                    data = GUI_String_Attr(name, (string)data, space);
                }
                else if (type.BaseType == TypeEnum)
                {
                    data = GUI_Enum_Attr(name, (Enum)data, type, true, space);
                }
            }
            else if (type.IsGenericType)
            {
                Type[] types = type.GenericTypeArguments;
                if (types.Length == 1 && type == typeof(List<>).MakeGenericType(types))
                {
                    if (data == null)
                    {
                        Type generacType = typeof(List<>).MakeGenericType(types);
                        {
                            data = generacType.Assembly.CreateInstance(generacType.FullName);
                        }
                    }
                    MethodInfo method = GUI_List_Method;
                    MethodInfo gui_list = method.MakeGenericMethod(types);
                    string des = EditorHook.description.GetDescription(name, type);
                    gui_list.Invoke(null, new object[] { data, des, space });
                }

                if (types.Length == 1 && type == typeof(RepeatedField<>).MakeGenericType(types))
                {
                    if (data == null)
                    {
                        Type generacType = typeof(List<>).MakeGenericType(types);
                        {
                            data = generacType.Assembly.CreateInstance(generacType.FullName);
                        }
                    }
                    MethodInfo method = GUI_RepeatedField_Method;
                    MethodInfo gui_list = method.MakeGenericMethod(types);
                    string des = EditorHook.description.GetDescription(name, type);
                    gui_list.Invoke(null, new object[] { data, des, space, null });
                }
                else if (types.Length == 2 && type == typeof(Dictionary<,>).MakeGenericType(types))
                {
                    MethodInfo method = GUI_Dict_Method;
                    MethodInfo gui_Dict = method.MakeGenericMethod(types);
                    string des = EditorHook.description.GetDescription(name, type);
                    gui_Dict.Invoke(null, new object[] { data, des, space });
                }
            }
            else if (type.IsClass && !type.IsArray)
            {
                if (data == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(space);
                    if (GUILayout.Button("Create:" + type.Name))
                    {
                        if (type.IsArray)
                        {
                            data = type.Assembly.CreateInstance(type.FullName, false, BindingFlags.CreateInstance, null, new object[] { 1 }, null, null);
                        }
                        else
                        {
                            data = type.Assembly.CreateInstance(type.FullName);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (GUI_Draw_Breviary(data, name, space))
                {
                    space += SpaceCount;
                    if (ReflectFieldMode)
                    {
                        Dictionary<string, FieldInfo> fields = ReflectHelper.GetFields(type);
                        GUILayout.BeginHorizontal();
                        foreach (var field in fields)
                        {
                            FieldInfo item = field.Value;
                            string des = EditorHook.description.GetDescription(item.Name, type);
                            if (string.IsNullOrEmpty(des)) des = item.Name;
                            object value = item.GetValue(data);
                            if (!CheckAttributes(item, des, ref value, space) && !CheckCustomizeGUI(item, des, ref value, space))
                            {
                                item.SetValue(data, GUI_Draw(item.FieldType, value, des, space));
                            }
                            else
                            {
                                item.SetValue(data, value);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    else if (GUI_Draw_Breviary(data, name, space))
                    {
                        Dictionary<string, PropertyInfo> properties = ReflectHelper.GetProperties(type);
                        GUILayout.BeginVertical();
                        foreach (var property in properties)
                        {
                            PropertyInfo item = property.Value;
                            if ((!item.CanRead || !item.CanWrite) && item.PropertyType == TypeBool)
                                continue;
                            string des = EditorHook.description.GetDescription(item.Name, type);
                            if (string.IsNullOrEmpty(des)) des = item.Name;
                            object value = item.GetValue(data);
                            if (!CheckAttributes(item, des, ref value, space) && !CheckCustomizeGUI(item, des, ref value, space))
                            {
                                if (item.CanWrite)
                                    item.SetValue(data, GUI_Draw(item.PropertyType, value, des, space));
                                else
                                    GUI_Draw(item.PropertyType, value, des, space);
                            }
                            else
                            {
                                item.SetValue(data, value);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
            else if (type.IsArray)
            {
                if(data == null)
                {
                    data = type.Assembly.CreateInstance(type.FullName,false,BindingFlags.CreateInstance,null,new object[] { 1},null,null);
                }
                Type elemetType = type.Assembly.GetType(type.FullName.Replace("[]",""));

                MethodInfo method = GUI_Array_Method;
                MethodInfo gui_Array = method.MakeGenericMethod(new Type[] { elemetType});
                string des = EditorHook.description.GetDescription(name,type);
                data = gui_Array.Invoke(null,new object[] {data,des,space });
            }
            return data;
        }


        public static T[] GUI_Array<T>(T[] data,string name,int space)
        {
            if (data == null) return null;
            if(GUI_Draw_Breviary(data,name+":"+data.Length,space))
            {
                space += SpaceCount;
                Type type = typeof(T);
                for(int i=0;i<data.Length;i++)
                {
                    T item = data[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    if (type.IsValueType)
                        data[i] = (T)GUI_Draw(type, item, i.ToString(), space);
                    else
                        GUI_Draw(type,item,i.ToString(),space);
                    GUILayout.EndVertical();
                    if(GUILayout.Button("-",GUILayout.Width(SmallButtonSize)))
                    {
                        List<T> list = new List<T>();
                        list.AddRange(data);
                        list.Remove(item);
                        data = list.ToArray();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(space);
                if(GUILayout.Button("+"))
                {
                    T[] newData = new T[data.Length+1];
                    for(int i=0;i<data.Length;i++)
                    {
                        newData[i] = data[i];
                    }
                    newData[data.Length] = (T)type.Assembly.CreateInstance(type.FullName);
                }
                EditorGUILayout.EndHorizontal();
            }
            return data;
        }
        /// <summary>
        /// list对象的GUI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="space"></param>
        public static void GUI_List<T>(List<T> data,string name,int space)
        {
            if (data == null) return;
            if(GUI_Draw_Breviary(data,name+":"+data.Count,space))
            {
                space += SpaceCount;
                Type type = typeof(T);
                for(int i=0;i<data.Count;i++)
                {
                    T item = data[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    if (type.IsValueType)
                        data[i] = (T)GUI_Draw(type, item, i.ToString(), space);
                    else
                        GUI_Draw(type,item,i.ToString(),space);
                    GUILayout.EndVertical();
                    if(GUILayout.Button("-",GUILayout.Width(SmallButtonSize)))
                    {
                        data.Remove(item);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(space);
                if(GUILayout.Button("+"))
                {
                    Type typeListT = typeof(List<T>);
                    MethodInfo methodAdd = typeListT.GetMethod("Add");
                    if(methodAdd !=null)
                    {
                        object newobj = type.Assembly.CreateInstance(type.FullName);
                        methodAdd.Invoke(data,new object[] {newobj });
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        
        public static void GUI_RepeateField<T>(RepeatedField<T> data,string name,int space,Func<T,string,int,T> func= null)
        {
            if (data == null) return;
            if (GUI_Draw_Breviary(data, name + ":" + data.Count, space))
            {
                space += SpaceCount;
                Type type = typeof(T);
                for (int i = 0; i < data.Count; i++)
                {
                    T item = data[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    if (type.IsValueType)
                        data[i] = func.Invoke(item, i.ToString(), space);
                    else
                        data[i] = (T)GUI_Draw(type, item, i.ToString(), space);
                    GUILayout.EndVertical();
                    if (GUILayout.Button("-", GUILayout.Width(SmallButtonSize)))
                    {
                        data.Remove(item);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(space);
                if (GUILayout.Button("+"))
                {
                    Type typeListT = typeof(RepeatedField< T >);
                    MethodInfo methodAdd = typeListT.GetMethod("Add",new Type[] { typeof(T)});
                    if (methodAdd != null)
                    {
                        object newobj = null;
                        if (type == typeof(string))
                            newobj = "";
                        else
                            newobj = type.Assembly.CreateInstance(type.FullName);
                        methodAdd.Invoke(data, new object[] { newobj });
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// dictionary对象的GUI
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="space"></param>
        public static void GUI_Dick<K,V>(Dictionary<K,V> data,string name,int space)
        {
            if (data == null) return;
            if (GUI_Draw_Breviary(data, name + ":" + data.Count, space))
            {
                space += SpaceCount;
                Type type = typeof(V);
                List<Type> desTypes = new List<Type>();
                bool valid = Description.TryGetDesType(name,desTypes);

                string des = "";
                Dictionary<K, V>.KeyCollection keyco = data.Keys;
                foreach(var key in keyco)
                {
                    des = key.ToString();
                    if(valid && desTypes[0].IsEnum)
                    {
                        des = desTypes[0].GetEnumName(key);
                        des = EditorHook.description.GetDescription(des,desTypes[0]);
                    }

                    V value = (V)GUI_Draw(type,data[key],des,space);
                    if(type.IsValueType&& !value.Equals(data[key]))
                    {
                        data[key] = value;
                        break;
                    }
                }
            }
        }

        public static bool CheckAttributes(Type type,ref object data,string name,int space)
        {
            bool result = false;

            var attrCustomGUI = type.GetCustomAttribute<GameFrame.CustomizeGUIAttribute>();
            if(attrCustomGUI == null)
            {
                CustomGUI.typeDict.TryGetValue(type,out attrCustomGUI);
            }
            if(attrCustomGUI != null)
            {
                Type classType = Type.GetType(attrCustomGUI.typeName);
                if(classType != null)
                {
                    MethodInfo method = classType.GetMethod(attrCustomGUI.methodName,attrCustomGUI.flag);
                    if(method != null)
                    {
                        if(GUI_Draw_Breviary(data,name,space))
                        {
                            space += SpaceCount;
                            data = method.Invoke(null,new object[] { data,space});
                        }
                    }
                }
                result = true; 
            }
            var attrHideInInspector = type.GetCustomAttribute<HideInInspector>();
            if(attrHideInInspector!= null)
            {
                result = true;
            }
            return result;
        }

        private static bool CheckAttributes(FieldInfo field,string des,ref object data,int space)
        {
            bool result = false;

            var attrCustomGUI = field.GetCustomAttribute<GameFrame.CustomizeGUIAttribute>();
            if (attrCustomGUI != null)
            {
                Type classType = Type.GetType(attrCustomGUI.typeName);
                if (classType != null)
                {
                    MethodInfo method = classType.GetMethod(attrCustomGUI.methodName, attrCustomGUI.flag);
                    if (method != null)
                    {
                        data = method.Invoke(null, new object[] {des, data, space });
                    }
                }
                result = true;
            }
            var attrHideInInspector = field.GetCustomAttribute<HideInInspector>();
            if (attrHideInInspector != null)
            {
                result = true;
            }
            return result;
        }

        private static bool CheckAttributes(PropertyInfo property,string des,ref object data,int space)
        {
            bool result = false;

            var attrCustomGUI = property.GetCustomAttribute<GameFrame.CustomizeGUIAttribute>();
            if (attrCustomGUI != null)
            {
                Type classType = Type.GetType(attrCustomGUI.typeName);
                if (classType != null)
                {
                    MethodInfo method = classType.GetMethod(attrCustomGUI.methodName, attrCustomGUI.flag);
                    if (method != null)
                    {
                        data = method.Invoke(null, new object[] { des, data, space });
                    }
                }
                result = true;
            }
            var attrHideInInspector = property.GetCustomAttribute<HideInInspector>();
            if (attrHideInInspector != null)
            {
                result = true;
            }
            return result;
        }

        private static bool CheckCustomizeGUI(FieldInfo field,string des,ref object data,int space)
        {
            bool result = false;
            if(GameEditor.CustomGUI.fieldDict.TryGetValue(field,out Attribute attr))
            {
                if(attr !=null)
                {
                    if(attr is CustomizeGUIAttribute)
                    {
                        CustomizeGUIAttribute attrCustomGUI = attr as CustomizeGUIAttribute;
                        Type classType = Type.GetType(attrCustomGUI.typeName);
                        if(classType != null)
                        {
                            MethodInfo method = classType.GetMethod(attrCustomGUI.methodName,attrCustomGUI.flag);
                            if(method!=null)
                            {
                                data = method.Invoke(null,new object[] { des,data,space});
                            }
                        }
                        result = true;
                    }
                    if(attr is HideInInspector)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private static bool CheckCustomizeGUI(PropertyInfo property,string des,ref object data,int space)
        {
            bool result = false;
            if (GameEditor.CustomGUI.propertyDict.TryGetValue(property, out Attribute attr))
            {
                if (attr != null)
                {
                    if (attr is CustomizeGUIAttribute)
                    {
                        CustomizeGUIAttribute attrCustomGUI = attr as CustomizeGUIAttribute;
                        Type classType = Type.GetType(attrCustomGUI.typeName);
                        if (classType != null)
                        {
                            MethodInfo method = classType.GetMethod(attrCustomGUI.methodName, attrCustomGUI.flag);
                            if (method != null)
                            {
                                data = method.Invoke(null, new object[] { des, data, space });
                            }
                        }
                        result = true;
                    }
                    if (attr is HideInInspector)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
