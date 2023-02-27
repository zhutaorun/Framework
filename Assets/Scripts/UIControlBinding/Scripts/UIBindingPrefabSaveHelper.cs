using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;

namespace GameFrame.UITools
{
    public class UIBindingPrefabSaveHelper : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// 保存资源时修正控件绑定数据
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        static string[] OnWillSaveAssets(string[] paths)
        {
            GameObject goInHierarchy = Selection.activeGameObject;
            if(goInHierarchy != null)
            {
                UIControlData[] uiControlData = goInHierarchy.GetComponentsInChildren<UIControlData>();
                if (uiControlData != null)
                {
                    foreach (var comp in uiControlData)
                    {
                        bool isOK = comp.CorrectComponents();
                        isOK = comp.CheckSubUIs();
                    }
                }
            }

            return paths;
        }

        //static void StartInitializeOnLoadMethod()
        //{
        //    PrefabUtility.prefabInstanceUpdated += ProcessUIControlData;
        //}

        public static void SavePrefab(GameObject goInHierarchy)
        {
            UnityEngine.Object goPrefab = null;
            UnityEngine.GameObject objValid = null;
            UnityEngine.GameObject objToCheck = goInHierarchy;
            string prefabPath = null;
            do
            {
                var currPrefab = PrefabUtility.GetPrefabParent(goInHierarchy);
                if (currPrefab == null)
                    break;

                string currPath = AssetDatabase.GetAssetPath(currPrefab);
                if (prefabPath == null)
                    prefabPath = currPath;
                if (currPath != prefabPath) // 已经到Root或者当前是嵌套prefab并且已经到达了上一层prefab
                    break;
                goPrefab = currPrefab;
                objValid = objToCheck;

                var t = objToCheck.transform.parent;
                if (t != null)
                    objToCheck = t.gameObject;
                else
                    break;
            }
            while (true);

            if (objValid != null)
                PrefabUtility.ReplacePrefab(goInHierarchy, goPrefab, ReplacePrefabOptions.ConnectToPrefab);
            else
                Debug.LogFormat("<color=red>当前对象不属于Prefab, 请将其保存为 Prefab</color>");
        }

    //    public static void ClearConsole()
    //    {
    //#if UNITY_2017 || UNITY_2018
    //        var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
    //#else
    //        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
    //#endif
    //        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
    //        clearMethod.Invoke(null, null);
     //   }
    }

}
#endif