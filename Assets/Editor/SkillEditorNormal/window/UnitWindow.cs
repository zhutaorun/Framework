using System.Collections.Generic;
using UnitConfig;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class UnitWindow : EditorWindow
    {
        public static void Open()
        {
            EditorWindow window = GetWindow<UnitWindow>();
            window.Show();
        }

        private const string DEFAULT_INPUT_STRING = "搜索";
        private string m_InputString = DEFAULT_INPUT_STRING;
        private List<object> m_CreatureData;//角色配置
        private Vector2 m_ScrollPosition;

        private GUIContent m_CreateContent;
        private GUIContent m_SaveContent;
        private GUIContent m_DeleteContent;

        private void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent("Unit","d_CustomTool");
            m_CreateContent = new GUIContent(" 添 加 ",InnerIcon.CreateAddNew,"Add a new unit data");
            m_SaveContent = new GUIContent(" 保 存 ",InnerIcon.Save,"Save config to disk");
            m_DeleteContent = new GUIContent(" 删 除 ",InnerIcon.Trash,"Delete Current Unit");
            if (m_CreateContent == null)
            {
                EditorHook.configCtrl.CreateData<unit_data>();
                m_CreatureData = EditorHook.configCtrl.GetListData<unit_data>();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUI_Function();
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            foreach(var item in m_CreatureData)
            {
                unit_data unit = item as unit_data;
                if(m_InputString != string.Empty && m_InputString != DEFAULT_INPUT_STRING && !(unit.Id.ToString().Contains(m_InputString) || unit.Name.Contains(m_InputString)))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                AttrGUIHelper.GUI_Draw(unit,unit.Id+". "+unit.Name);
                GUILayout.EndVertical();
                if(GUILayout.Button(m_DeleteContent,GUILayout.Width(100)))
                {
                    m_CreatureData.Remove(item);
                    break;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        private void GUI_Function()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(m_CreateContent,GUILayout.Height(40)))
            {
                m_CreatureData.Add(new unit_data());
            }
            GUILayout.Space(5);
            if(GUILayout.Button(m_SaveContent,GUILayout.Height(40)))
            {
                SaveFile();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            m_InputString = EditorGUILayout.TextField(m_InputString, EditorStyles.toolbarSearchField, GUILayout.Height(20));
        }

        private void SaveFile()
        {
            UnitConfig.container container = new UnitConfig.container();
            foreach(var item in m_CreatureData)
            {
                unit_data unit = item as unit_data;
                if(container.Infos.ContainsKey(unit.Id))
                {
                    EditorUtility.DisplayDialog("Error",string.Format("存在重复的key{0},保存失败",unit.Id),"确定");
                    return;
                }
                container.Infos.Add(unit.Id,unit);
            }
            SkillController.SkillCfg.SaveFile(SkillDefine.UnitConfigPath,container);
            CheckRepeatedSkillId(container);
            AssetDatabase.Refresh();
        }


        private void CheckRepeatedSkillId(UnitConfig.container container)
        {
            Dictionary<int, int> list = new Dictionary<int, int>();
            foreach(var item in container.Infos)
            {
                foreach(var sk in item.Value.Skills)
                {
                    if(sk ==0)
                    {
                        EditorUtility.DisplayDialog("错误",$"id为{item.Value.Id}的角色的技能列表存在id为0的技能","确定");
                        return;
                    }
                    if(list.ContainsKey(sk))
                    {
                        EditorUtility.DisplayDialog("错误", $"id为{item.Value.Id}的角色与id为{list[sk]}的角色存在重复的技能id:{sk}", "确定");
                        return;
                    }
                    list.Add(sk,item.Value.Id);
                }
            }
        }
    }
}
