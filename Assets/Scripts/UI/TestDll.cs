using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;

public class TestDll : MonoBehaviour
{
    [DllImport("DLLTest", CharSet = CharSet.Auto, EntryPoint = "UnityDLLTest")]
    public static extern int UnityDLLTest();
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("测试下C"+ UnityDLLTest());
        //Debug.Log(Application.unityVersion);

        //Type type = Type.GetType("Mono.Runtime");
        //if (type != null)
        //{
        //    MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
        //    if (displayName != null)
        //        Debug.Log(displayName.Invoke(null, null));

        //    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        //    for (int i = 0; i < methods.Length; i++)
        //    {
        //        MethodInfo m = methods[i];
        //        Debug.Log((m.IsPublic ? "public " : (m.IsPrivate ? "private " : "")) + (m.IsStatic ? "static " : " ") + m.ReturnType.Name + " " + m.Name + " " + m.GetParameters().Length);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
