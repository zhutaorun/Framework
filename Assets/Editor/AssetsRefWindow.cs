using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetsRefWindow : EditorWindow
{
    [MenuItem("Assets/查看资源引用")]
    public static void FindAssetsRef()
    {
        if(Selection.assetGUIDs.Length == 0)
        {
            Debug.Log("请选中任意资源!");
            return;
        }


        string[] assetGUIDs = Selection.assetGUIDs;

        string[] assetPaths = new string[Selection.assetGUIDs.Length];
        for(int i=0;i< Selection.assetGUIDs.Length;i++)
        {
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
        }

        Dictionary<string, List<string>> refPaths = new Dictionary<string, List<string>>();

        var allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach(var path in allAssetPaths)
        {
            if(path.EndsWith(".unity")|| path.EndsWith(".prefab")|| path.EndsWith(".mat")|| path.EndsWith(".controller"))
            {
                string content = File.ReadAllText(path);
                if (string.IsNullOrEmpty(content)) continue;

                foreach(var guid in assetGUIDs)
                {
                    int index = content.IndexOf(guid);
                    while(index >0)
                    {
                        string nameStr = content.Substring(0,index);
                        int nameIndex = nameStr.IndexOf("m_Name");
                        string name = "";

                        while(nameIndex>0)
                        {
                            for(int i=nameIndex+7;i<nameStr.Length;i++)
                            {
                                name += nameStr[i];
                                if (nameStr[i] == '\n'|| nameStr[i] =='\t')
                                {
                                    break;
                                }
                            }

                            if(string.IsNullOrEmpty(name.Trim()))
                            {
                                nameStr = nameStr.Substring(0,nameIndex);
                                nameIndex = nameStr.LastIndexOf("m_Nmae:");
                                name = "";
                            }
                            else
                            {
                                break;
                            }
                        }

                        if(!refPaths.TryGetValue(path,out var list))
                        {
                            list = new List<string>();
                            refPaths.Add(path,list);
                        }
                        list.Add(name.Trim());

                        content = content.Substring(index+guid.Length,content.Length - index - guid.Length);
                        index = content.IndexOf(guid);
                    }
                }
            }
        }

        AssetsRefWindow assetsRefWindow = GetWindow<AssetsRefWindow>();
        if(assetsRefWindow== null)
        {
            assetsRefWindow = CreateWindow<AssetsRefWindow>();
        }
        assetsRefWindow.refPaths = refPaths;
        assetsRefWindow.assetPaths = assetPaths;
        assetsRefWindow.Focus();
    }


    public string[] assetPaths;
    public Dictionary<string, List<string>> refPaths;
    private Dictionary<string, bool> isFoldMap = new Dictionary<string, bool>();
    private Vector2 ScrollView;

    private void OnGUI()
    {
        ScrollView = EditorGUILayout.BeginScrollView(ScrollView);

        if (assetPaths != null)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                EditorGUILayout.ObjectField("选中的资源", AssetDatabase.LoadAssetAtPath<Object>(assetPaths[i]), typeof(Object), true);
            }
        }
        EditorGUILayout.Space(20);



        if (refPaths != null)
        {
            EditorGUILayout.LabelField("资源引用列表：");

            foreach (var item in refPaths)
            {
                EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Object>(item.Key), typeof(Object), true);
                if (!isFoldMap.ContainsKey(item.Key))
                    isFoldMap.Add(item.Key, false);
                if (isFoldMap[item.Key] = EditorGUILayout.Foldout(isFoldMap[item.Key], "查看引用名称:"))
                {
                    foreach (var str in item.Value)
                    {
                        EditorGUILayout.TagField(str);
                    }
                }
                EditorGUILayout.Space(5);
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
