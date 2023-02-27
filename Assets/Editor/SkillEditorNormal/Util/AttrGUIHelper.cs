/*
 //  公共的属性绘制类
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillnewConfig;
using System;
using UnityEditor;

namespace GameEditor
{
    public static partial class AttrGUIHelper 
    {
        private static Type TypeInt = typeof(int);
        private static Type TypeUInt = typeof(uint);
        private static Type TypeString = typeof(string);
        private static Type TypeFloat = typeof(float);
        private static Type TypeDouble = typeof(double);
        private static Type TypeEnum = typeof(Enum);
        private static Type TypeBool = typeof(bool);
        private static Type TypeUShort = typeof(ushort);


        /// <summary>
        /// 增删类小按钮size
        /// </summary>
        private const float SmallButtonSize = 20;

        /// <summary>
        /// 空白大小
        /// </summary>
        private const int SpaceCount = 20;


        private static Dictionary<string, bool> listBreviaryValue = new Dictionary<string, bool>();

        public static bool GetClickedValue(string name,bool defaultValue = true)
        {
            bool result = true;//除了treeView外，默认展开属性
            if(!listBreviaryValue.TryGetValue(name,out result))
            {
                result = defaultValue;
                listBreviaryValue.Add(name,result);
            }
            return result; 
        }

        private static void SetClickedValue(string name,bool value)
        {
            if(listBreviaryValue.ContainsKey(name))
            {
                listBreviaryValue[name] = value;
            }
        }

        #region base type gui
        public static int GUI_Int_Attr(int value)
        {
            value = EditorGUILayout.IntField(value);
            return value;
        }

        //Int
        public static int GUI_Int_Attr(string label,int value,int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.IntField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static int GUI_Int_Attr(string name,int value,Type type,int space =0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if(des!=null && des!= string.Empty)
                EditorGUILayout.LabelField(des);
            else if(name != string.Empty)
                EditorGUILayout.LabelField(name);
            value = EditorGUILayout.IntField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static int GUI_Int_Attr<T>(string name, int value,int space=0)
        {
            return GUI_Int_Attr(name,value,typeof(T),space);
        }


        public static int GUI_Int_Btn_Attr(string name,int value,Type type,int space =0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);

            if (GUILayout.Button("_",GUIStyleExtension.buttonStyleMid))
            {
                value--;
            }

            value = EditorGUILayout.IntField(value);

            if (GUILayout.Button("+", GUIStyleExtension.buttonStyleMid))
            {
                value++;
            }
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static int GUI_Int_Btn_Attr<T>(string name,int value,int space = 0)
        {
            return GUI_Int_Btn_Attr(name,value,typeof(T),space);
        }

        //UInt
        public static uint GUI_UInt_Attr(string label, uint value, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            EditorGUILayout.LabelField(label);
            value = (uint)EditorGUILayout.IntField((int)value);

            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static uint GUI_UInt_Attr(string name,uint value, Type type, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            value = (uint)EditorGUILayout.IntField((int)value);

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static uint GUI_UInt_Attr<T>(string name, uint value, int space = 0)
        {
            return GUI_UInt_Attr(name, value, typeof(T), space);
        }
        //float
        public static float GUI_Float_Attr(string label, float value, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            EditorGUILayout.LabelField(label);
            value = (uint)EditorGUILayout.FloatField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static float GUI_Float_Attr(string name, float value, Type type, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            value = (uint)EditorGUILayout.FloatField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static float GUI_Float_Attr<T>(string name, float value, int space = 0)
        {
            return GUI_Float_Attr(name, value, typeof(T), space);
        }

        //sting
        public static string GUI_String_Attr(string label, string value, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.TextField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static string GUI_String_Attr(string name, string value, Type type, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            value = EditorGUILayout.TextField(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static string GUI_String_Attr<T>(string name, string value, int space = 0)
        {
            return GUI_String_Attr(name, value, typeof(T), space);
        }

        //Enum

        public static Enum GUI_Enum_Attr(string name, Enum @enum, Type type, bool useHorizon = true, int space = 0)
        {
            if (useHorizon)
            { 
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(space);
            }
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            Type enumType = @enum.GetType();
            Array array = Enum.GetValues(enumType);
            int index = Array.IndexOf(array,@enum);
            string[] desArray = EditorHook.description.GetEnumDesArray(enumType.Name);
            if(desArray != null && desArray.Length>0)
            {
                int selectIndex = EditorGUILayout.Popup(index,desArray);
                @enum = (Enum)array.GetValue(selectIndex);
            }
            else
            {
                @enum = EditorGUILayout.EnumPopup(@enum);
            }
            if(useHorizon)
                EditorGUILayout.EndHorizontal();
            return @enum;
        }

        public static Enum GUI_Enum_Attr<T>(string name, Enum @enum, int space = 0)
        {
            return GUI_Enum_Attr(name, @enum, typeof(T),true ,space);
        }
        //Bool
        public static bool GUI_Bool_Attr(string label, bool value, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);

            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.Toggle(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }


        public static bool GUI_Bool_Attr(string name, bool value, Type type, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            value = EditorGUILayout.Toggle(value);

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static bool GUI_Bool_Attr<T>(string name, bool value, int space = 0)
        {
            return GUI_Bool_Attr(name, value, typeof(T), space);
        }

        //Gloabl_Int_Vector3

        public static Global_Int_Vector3 GUI_IntVector3(string name, Global_Int_Vector3 value, Type type, int space = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            string des = EditorHook.description.GetDescription(type.Name, name);
            if (des != null && des != string.Empty)
                EditorGUILayout.LabelField(des);
            else if (name != string.Empty)
                EditorGUILayout.LabelField(name);
            if (value == null) value = new Global_Int_Vector3();
            int x=0, y=0, z = 0;
            x = EditorGUILayout.IntField(value.X);
            y = EditorGUILayout.IntField(value.Y);
            z = EditorGUILayout.IntField(value.Z);

            value.X = x;
            value.Y = y;
            value.Z = z;

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static Global_Int_Vector3 GUI_IntVector3<T>(string name, Global_Int_Vector3 value, int space = 0)
        {
            return GUI_IntVector3(name, value, typeof(T), space);
        }

        #endregion


        public static bool GUI_Draw_Breviary(object data,string des,int space = 0,bool defaultDisplay = true)
        {
            int hasCode = data.GetHashCode();
            bool isClicked = GetClickedValue(hasCode.ToString(),defaultDisplay);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(space);
            isClicked = EditorGUILayout.Foldout(isClicked,des,true);
            SetClickedValue(hasCode.ToString(),isClicked);
            EditorGUILayout.EndHorizontal();
            return isClicked;
        }
    }

}