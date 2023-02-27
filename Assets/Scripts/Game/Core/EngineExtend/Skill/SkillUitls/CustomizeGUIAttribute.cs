using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 自定义GUI方法属性
    /// </summary>
    public class CustomizeGUIAttribute : Attribute
    {
        /// <summary>
        /// 方法所在类型名
        /// </summary>
        public string typeName;//注:使用时不能直接使用editor下的类型，故在此处使用string

        /// <summary>
        /// GUI方法名
        /// </summary>
        public string methodName;


        /// <summary>
        /// flag
        /// </summary>
        public BindingFlags flag;


        public CustomizeGUIAttribute(string typeName,string methodName,BindingFlags flag = BindingFlags.Public)
        {
            this.typeName = typeName;
            this.methodName = methodName;
            this.flag = flag | BindingFlags.Static;
        }
    }

}