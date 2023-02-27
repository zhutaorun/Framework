using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SkillnewConfig;

namespace GameEditor
{
    public class ActionListWindow : EditorWindow
    {
        private List<string> SkillPath;
        private List<string> CommonPath;
        private List<string> SkillTestPath;
        private Dictionary<string, List<MotionInfo>> RootMotionDic;
        private Vector2 pos = Vector2.zero;
        private Texture[] m_UITexture = new Texture[3];//工具栏里使用的play,pause,stop
        private bool isPlaying = false;
        private double m_Time = 0;
        private float m_TotalTime;
        private string m_AnimName = string.Empty;
        private bool m_rootMotionActs = false;

        private void OnEnable()
        {
            SkillPath = EditorHook.editorModelCtrl.CollectSkillPath();
            CommonPath = EditorHook.editorModelCtrl.CollectCommonPath();
            m_UITexture[0] = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ArtRoot/EditorPic/tex_play.png");
            m_UITexture[1] = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ArtRoot/EditorPic/tex_stop.png");
            m_UITexture[2] = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ArtRoot/EditorPic/tex_forward.png");
        }

        private void Update()
        {
            if(isPlaying&& m_TotalTime>0)
            {
                m_Time += Time.deltaTime;
                if (m_Time > m_TotalTime)
                    m_Time = 0;
                SetAnimation();
            }
        }

        private void OnDisable()
        {
            isPlaying = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            DrawStopBtn();
            DrawPlayBtn();
            DrawGenerateRootMotionBtn();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            pos = GUILayout.BeginScrollView(pos);
            {
                if(m_rootMotionActs)
                {
                    foreach(var item in SkillTestPath)
                    {
                        if (GUILayout.Button(item))
                        {
                            m_AnimName = item;
                            SetClip(true);
                            Play();
                        }
                    }
                }
                else
                {
                    foreach(var item in SkillPath)
                    {
                        if(GUILayout.Button(item))
                        {
                            m_AnimName = item;
                            SetClip(true,false);
                            Play();
                        }
                    }

                    foreach(var item in CommonPath)
                    {
                        if(GUILayout.Button(item))
                        {
                            m_AnimName = item;
                            SetClip(false, false);
                            Play();
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SetClip(bool isSkill,bool test = true)
        {
            EditorHook.editorModelCtrl.LoadClipAsync(m_AnimName, isSkill, (animationClip) => 
            {
                m_TotalTime = animationClip.length;
            },test);
        }

        private void SetAnimation()
        {
            if(m_AnimName != string.Empty)
            {
                EditorHook.editorModelCtrl.SetClipTime(m_AnimName, (float)m_Time);
                if (RootMotionDic != null)
                    EditorHook.editorModelCtrl.SampleModelPosition(RootMotionDic[m_AnimName], (float)m_Time, Vector3.zero);
            }
        }

        private void DrawStopBtn()
        {
            if(GUILayout.Button(m_UITexture[1],GUILayout.Height(40)))
            {
                isPlaying = false;
                Stop();
            }
        }

        /// <summary>
        /// 播放按钮
        /// </summary>
        private void DrawPlayBtn()
        {
            Texture texture = isPlaying ? m_UITexture[2] : m_UITexture[0];

            if(GUILayout.Button(texture,GUILayout.Height(40)))
            {
                isPlaying = !isPlaying;
            }
        }

        private void DrawGenerateRootMotionBtn()
        {
            if(GUILayout.Button("采集RootMotion",GUILayout.Height(40)))
            {
                if (RootMotionDic == null)
                    RootMotionDic = new Dictionary<string, List<MotionInfo>>();
                RootMotionDic.Clear();

                m_rootMotionActs = true;
                SkillTestPath = EditorHook.editorModelCtrl.CollectSkillTestPath();

                for(int i=0;i<SkillTestPath.Count;i++)
                {
                    string path = SkillTestPath[i];
                    if(!RootMotionDic.ContainsKey(path))
                    {
                        List<MotionInfo> motionList = EditorHook.editorModelCtrl.NewCollectSkillActionMotions(path);
                        RootMotionDic.Add(path,motionList);
                    }
                }
            }
        }

        private void Play()
        {
            isPlaying = true;
        }

        private void Stop()
        {
            m_Time = 0;
            isPlaying = false;
        }
    } 
}