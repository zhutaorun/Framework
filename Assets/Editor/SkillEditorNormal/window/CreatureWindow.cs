using GameFrame;
using System;
using System.Collections.Generic;
using UnitConfig;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class CreatureWindow : EditorWindow
    {
        private const string DEFAULT_INPUT_STRING = "搜索";

        private List<object> m_CreatureData;    //角色配置
        private unit_data m_CurrentData = null;
        private Vector2 m_Scroll = Vector2.zero;
        private string m_InputString = DEFAULT_INPUT_STRING;
        private int m_SortType = (int)SortType.默认顺序;
        private DateTime time = DateTime.MinValue;

        enum SortType : byte
        {
            默认顺序,
            标准优先,
            boss优先,
        }

        private void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent("Creature", "d_CustomTool");
            Initialize();

            EditorApplication.update += SelfUpdate;
        }

        private void Initialize()
        {
            Hooks.CreateManager();
            EditorHook.Initialize();
            EditorHook.configCtrl.LoadStartConfig();

            m_CreatureData = EditorHook.configCtrl.GetListData<unit_data>();

            //添加一个木桩
            EditorHook.editorSkillDisplayManager.GetCtrl<GameFrame.SkillDisplay.PunchingBagCtrl>().PrepareModel(1);
        }

        private void OnDisable()
        {
            EditorApplication.update -= SelfUpdate;
            Dispose();
        }

        private void Dispose()
        {
            SkillEditorWindowMenu.CloseAll(this);
            EditorHook.Release();
            m_CreatureData = null;
            m_CurrentData = null;
            SkillRangeShow.Instance.Dispose();
            GameFrame.ReflectHelper.Dispose();
            Hooks.ResourceManager.Cleanup();
        }

        private void SelfUpdate()
        {
            if (!EditorHook.initialized)
            {
                return;
            }

            if (time <= DateTime.MinValue)
            {
                time = DateTime.UtcNow;
                return;
            }

            //在没有unit文件时，oneable获取不到数据
            if (m_CreatureData == null)
            {
                m_CreatureData = EditorHook.configCtrl.GetListData<unit_data>();
            }

            DateTime now = DateTime.UtcNow;
            float fDeltaTime = (float)((now - time).TotalMilliseconds / 1000f);
            time = now;

            EditorHook.editorSkillDisplayManager.TryUpdateSkill(fDeltaTime);
        }

        private void OnGUI()
        {
            float width = this.position.width;
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(15);
                GUIWindowCtrl();
                if (GUILayout.Button("配置角色 "))
                {
                    UnitWindow.Open();
                }
                if (m_CreatureData != null && m_CreatureData.Count > 0)
                {
                    m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
                    {
                        CreatureManageGUI();
                        GUILayout.Space(5);
                        DrawCreatureIDBtn();
                        GUILayout.Space(5);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void CreatureManageGUI()
        {
            if (GUILayout.Button(new GUIContent(" 保 存", InnerIcon.Save, "保存所有技能信息")))
            {
                EditorHook.configCtrl.LoadSkillConfig(0, true);
                EditorHook.skillDataManager.SaveSkillConfig();
            }

            EditorGUILayout.BeginHorizontal();
            m_InputString = EditorGUILayout.TextField(m_InputString, EditorStyles.toolbarSearchField, GUILayout.Height(20));
            int sortType = EditorGUILayout.Popup(m_SortType, Enum.GetNames(typeof(SortType)), GUILayout.Width(80), GUILayout.Height(20));
            if (sortType != m_SortType)
            {
                m_SortType = sortType;
                m_CreatureData.Sort((a, b) =>
                {
                    unit_data ua = a as unit_data;
                    unit_data ub = b as unit_data;
                    return CreatureSort((SortType)m_SortType, ua, ub);
                });
            }
            EditorGUILayout.EndHorizontal();
        }

        private int CreatureSort(SortType sType, unit_data a, unit_data b)
        {
            switch (sType)
            {
                case SortType.boss优先:
                    return b.UnitType == UnitType.Monster ? 1 : -1;
                case SortType.标准优先:
                    return b.Name.Contains("标准") ? 1 : -1;
            }
            return a.Id - b.Id;
        }

        private void GUIWindowCtrl()
        {
            GUILayout.BeginVertical();
            {
                if (m_CurrentData != null)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("打开动作列表"))
                    {
                        SkillEditorWindowMenu.OpenActionListWindow();
                    }
                }

                if (GUILayout.Button(new GUIContent("关闭", "关闭此界面")))
                {
                    Close();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawCreatureIDBtn()
        {
            Color defaultColor = GUI.backgroundColor;
            foreach(var item in m_CreatureData)
            {
                unit_data unit = item as unit_data;
                if(m_InputString != DEFAULT_INPUT_STRING && m_InputString != string.Empty)
                {
                    if (!unit.Id.ToString().Contains(m_InputString) && !unit.Name.Contains(m_InputString))
                        continue;
                }

                GUI.backgroundColor = unit.UnitType == UnitType.Pc ? defaultColor : Color.green;

                bool isCurent = item == m_CurrentData;
                GUIStyleExtension.ApplySkinStyle(isCurent? GUIStyleExtension.defaultButtonSelectColor:default(Color),default(Color),default(Color));
                if(GUILayout.Button(unit.Name))
                {
                    EditorHook.configCtrl.CheckLoadSkillConfig(unit.Id);

                    EditorHook.skillDataManager.InitSkill(unit);
                    if(m_CurrentData != item)
                    {
                        m_CurrentData = unit;
                        EditorHook.editorSkillDisplayManager.GetCtrl<GameFrame.SkillDisplay.ModelCtrl>().PrepareModel((uint)m_CurrentData.Id);
                    }

                    OpenActionViews();
                }
            }
        }

        private void OpenActionViews()
        {
            SkillEditorWindowMenu.OpenActionOverView();
        }
    }
}