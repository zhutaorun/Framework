using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SkillnewConfig;
using System;
using AnimCurveConfig;

namespace GameEditor
{
    public class AnimCurveEditor : EditorWindow
    {
        public static AnimCurveEditor Instance = null;
        private AnimationCurve m_Curve;

        #region 编辑器下摄像机缩放
        private float m_CurZoomTime;
        private float m_CameraNowZoomValue;
        public bool CameraZoom = false;
        #endregion

        private string m_Input = "";

        public string Input //当前输入数据
        {
            get { return m_Input; }
            set
            {
                if (m_Input == value)
                    return;
                m_Input = value;
                CollectData();
            }
        }

        private Vector2 listScroll = Vector2.zero;
        private List<anim_curve_data> m_DataList = new List<anim_curve_data>();//展示用数据列表
        private anim_curve_data m_CurData = null;
        private int MaxID = 0;

        private List<object> m_All_data { get { return EditorHook.configCtrl.GetListData<anim_curve_data>(); } } //所有数据的集合
        private Dictionary<int, AnimationCurve> m_AnimCurveDict = new Dictionary<int, AnimationCurve>();
        private FrameEvent m_FrameEvent;
        private anim_curve_data m_AnimCurveData;
        private Dictionary<string, Action> m_OverrideAction = new Dictionary<string, Action>() { { "keyFrameList", null }, };

        private Dictionary<anim_curve_data, int> m_TempAnimIdDict = new Dictionary<anim_curve_data, int>();

        private GUILayoutOption[] animCurveLayoutOptions = new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(100) };

        private void OnEnable()
        {
            Instance = this;
            ReLoadConfig();
        }

        public void SetFrameEvent(FrameEvent frameEvent)
        {
            this.m_FrameEvent = frameEvent;
        }

        public void SetData(int id)
        {
            m_AnimCurveData = EditorHook.configCtrl.GetData<anim_curve_data>(id,"id") as anim_curve_data;
            if(m_AnimCurveData != null)
            {
                if(false == m_AnimCurveDict.ContainsKey(id))
                {
                    m_AnimCurveDict[id] = new AnimationCurve();
                    foreach(KeyFrame frame in m_AnimCurveData.KeyFrameList)
                    {
                        m_AnimCurveDict[id].AddKey(frame.Time,frame.Value);
                    }
                }
                m_Curve = m_AnimCurveDict[id];

                if (m_FrameEvent != null)
                    m_FrameEvent.FrameEventId = id;
            }
        }

        private void OnGUI()
        {
            GUI_Head();
            GUI_ListPart();
        }

        private void GUI_Head()
        {
            if(m_AnimCurveData !=null)
            {
                AttrGUIHelper.GUI_Draw(m_AnimCurveData,"anim_curve_data");
            }
            m_Curve = EditorGUILayout.CurveField(m_Curve,animCurveLayoutOptions);
            Input = EditorGUILayout.TextField(Input);
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    SaveConfig();
                }
                if (GUILayout.Button("Add"))
                {
                    AddConfig();
                }
                if (GUILayout.Button("Delete"))
                {
                    DeleteConfig();
                }
                if (GUILayout.Button("Reload"))
                {
                    ReLoadConfig();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void GUI_ListPart()
        {
            listScroll = EditorGUILayout.BeginScrollView(listScroll);
            {
                EditorGUILayout.BeginVertical();
                foreach(var item in m_DataList)
                {
                    bool isCurent = item == m_CurData;
                    GUIStyleExtension.ApplySkinStyle(isCurent? GUIStyleExtension.defaultButtonSelectColor:default(Color),default(Color),default(Color));
                    if(GUILayout.Button(item.Id.ToString()+" "+item.Name))
                    { 
                        if(m_CurData !=null && m_CurData.Id == item.Id)
                        {
                            SetData(item.Id);
                            CameraZoom = true;
                        }
                        m_CurData = item;
                    }
                    GUIStyleExtension.RecoverSkinStyle();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void ReLoadConfig()
        {
            SkillController.SkillCfg.LoadConfig<anim_curve_data>("AnimCurve@anim_cure");
            foreach(var item in m_All_data)
            {
                if ((item as anim_curve_data).Id > MaxID)
                    MaxID = (int)(item as anim_curve_data).Id;
            }
            Input = "";
            m_AnimCurveDict.Clear();
            CollectData();
        }

        private void CollectData()
        {
            if(m_All_data == null || m_All_data.Count<=0)
                return;
            m_DataList.Clear();
            foreach(var item in m_All_data)
            {
                if ((item as anim_curve_data).Id.ToString().Contains(m_Input))
                {
                    m_DataList.Add((item as anim_curve_data));
                }
            }

            if(m_DataList.Count>0)
            {
                m_CurData = m_DataList[0];
                SetData(m_CurData.Id);
            }
        }

        private void SaveConfig()
        {
            string bytePath = Application.dataPath + "/AssetsPackage/" + SkillDefine.BytesConfig + "AnimCurve@anim_curve.bytes";
            string jsonPath = Application.dataPath + "/AssetsPackage/" + SkillDefine.JsonConfig + "AnimCurve@anim_curve.json";

            AnimCurveConfig.container container = new AnimCurveConfig.container();
            List<anim_curve_data> list = new List<anim_curve_data>();

            foreach(var item in m_All_data)
            {
                container.Infos.Add((int)(item as anim_curve_data).Id, (item as anim_curve_data));
                list.Add((item as anim_curve_data));
            }

            foreach(anim_curve_data data in list)
            {
                Keyframe[] frames = null;
                if (m_AnimCurveDict.ContainsKey(data.Id))
                    frames = m_AnimCurveDict[data.Id].keys;
                else
                {
                    if(m_TempAnimIdDict.ContainsKey(data))
                    {
                        if (m_AnimCurveDict.ContainsKey(m_TempAnimIdDict[data]))
                            frames = m_AnimCurveDict[m_TempAnimIdDict[data]].keys;
                    }
                }

                if(frames!=null)
                {
                    data.KeyFrameList.Clear();
                    foreach(Keyframe keyframe in frames)
                    {
                        KeyFrame frame = new KeyFrame();
                        frame.Time = keyframe.time;
                        frame.Value = keyframe.value;
                        data.KeyFrameList.Add(frame);
                    }
                }
            }

            SkillController.SkillCfg.SaveFile(bytePath,container);
            AssetDatabase.Refresh();
        }

        public void AddConfig()
        {
            anim_curve_data data = new anim_curve_data();
            data.Id = (++MaxID);
            m_TempAnimIdDict[data] = data.Id;
            CollectData();
            m_CurData = data;
        }

        public void DeleteConfig()
        {
            if (m_CurData == null) return;
            m_All_data.Remove(m_CurData);
            m_DataList.Remove(m_CurData);
            m_CurData = null;
            Input = "";
        }

        public Vector3 UpdateCameraZoom(Transform @camera,float fDeltaTime)
        {
            if(m_CurData != null)
            {
                m_CurZoomTime += fDeltaTime;
                //摄像机缩放时间单位帧转秒
                float maxTime = m_Curve.keys[m_Curve.length - 1].time / (float)SkillDefine.FRAMENUM_PERSECOND;
                if(m_CurZoomTime>maxTime)
                {
                    m_CurZoomTime = maxTime;
                }

                m_CameraNowZoomValue = m_Curve.Evaluate(m_CurZoomTime* (float)SkillDefine.FRAMENUM_PERSECOND);

                if(m_CurZoomTime >= maxTime)
                {
                    m_CurZoomTime = 0;
                    m_CameraNowZoomValue = 0;
                    CameraZoom = false;
                }
            }

            return m_CameraNowZoomValue * camera.forward;
        }
    }
}
