using SkillnewConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class EditorSkillSearchWindow :EditorWindow
    {
        private string input = "";

        private string Input
        {
            get { return input; }
            set
            {
                if (input == value)
                    return;
                input = value;
                CollectData();
            }
        }

        private List<skillnew_data> data = new List<skillnew_data>();

        public Action<skillnew_data> callBack = null;

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUI_InputField();
            GUI_Result();
            GUILayout.EndVertical();
        }

        private void OnEnable()
        {
            EditorHook.configCtrl.CheckLoadSkillConfig();
            CollectData();
        }

        private void GUI_InputField()
        {
            GUILayout.BeginHorizontal();
            Input = EditorGUILayout.TextField(input,EditorStyles.textField);

            GUILayout.EndHorizontal();
        }

        private void GUI_Result()
        {
            foreach(var item in data)
            {
                if(GUILayout.Button(item.Id.ToString()+" " +item.SkillName))
                {
                    if (callBack != null)
                        callBack(item);
                }
            }
        }


        private void CollectData()
        {
            data.Clear();
            {
                Dictionary<int, object> dict = SkillController.SkillCfg.GetAll<skillnew_data>();
                if (dict == null) return;
                foreach(var item in dict)
                {
                    if(item.Key.ToString().Contains(input))
                    {
                        data.Add(item.Value as skillnew_data);
                        if (data.Count > 40)
                            break;
                    }
                }
            }
        }
    }

}