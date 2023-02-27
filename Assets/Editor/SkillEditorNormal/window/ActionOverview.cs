using GameFrame;
using Google.Protobuf.Collections;
using SkillnewConfig;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class ActionOverview : EditorWindow
    {
        public static ActionOverview Instance = null;

        private float fx = 0;//当前绘制的坐标位置
        private float fy = 0;//当前绘制的坐标位置

        #region constvalue
        private const int MARGIN_X = 30;
        private const int MARGIN_Y = 10;
        private const int MAX_ORDER = 10;
        private const int VERTICAL_GRID_LINE = 5;//grid区分线在每几个Frame显示
        private const int DEFAULT_EVENT_WIDTH = 10;//事件的显示宽度
        private const int DEFAULT_EVENT_HEIGHT = 20;//事件的显示高度
        private const int FRAME_NUMBERS_HEIGHT = 12;//帧数 数字 高度
        private const float V_SPACE = 5;
        private const int TOOLBAR_HEIGHT = 50;
        private const int FRAME_SLIDER_HEIGHT = 17;//Player slider 高度
        private const int ACTION_HEIGHT = 15;
        public const int FROMREPEAT_FRAMEEVENT = 13148;//repeat事件产生的帧事件
        #endregion
        private bool IdRepeated = false;
        private int m_SkillIndex = -1;  //当前技能的索引
        private int m_ActionFrameOffset = 0;
        private int m_CurActionIndex = 0;
        private int m_TotalFrame = 20;  //总帧数
        private int m_TotalDamagePerecent = 0;//总伤害的百分比 
        private int m_WeaponId = 0;

        private List<int> m_Weapons = new List<int>();
        private List<string> m_WeaponNames = new List<string>();

        private int SkillIndex
        {
            get { return m_SkillIndex; }
            set { SetSkill(value); }
        }

        private List<string> m_SkillIDList = new List<string>();//技能列表
        private Vector2 m_Scroll = Vector2.zero;
        private Vector2 m_FrameScroll = Vector2.zero;

        private int CurrentFrame { get { return EditorHook.editorSkillDisplayManager.CurrentFrame; } }//当前帧

        /// <summary>
        /// 当前技能配置
        /// </summary>
        private skillnew_data CurSkill 
        { 
            get { return EditorHook.skillDataManager.CurSkillConfig; }
            set { EditorHook.skillDataManager.CurSkillConfig = value; } 
        }

        private SkillActionInfo m_Action4Copy;//复制时使用

        public void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent("ActionOverview","d_CustomTool");
            position = new Rect(position.x,position.y,600,600);
            FreshIDList();
            FreshSelfData();
            Instance = this;
            InitWeaponData();
            SkillIndex = 0;
        }

        private void OnDisable()
        {
            Stop(false);
        }

        private void SetSkill(int value,bool forceReset = false)
        {
            if (EditorHook.skillDataManager.SkillConfigList.Count <= 0) return;
            if (!forceReset && m_SkillIndex == value) return;
            if (value >= EditorHook.skillDataManager.SkillConfigList.Count) value = EditorHook.skillDataManager.SkillConfigList.Count - 1;
            if (value < 0) value = 0;

            m_SkillIndex = value;
            CurSkill = EditorHook.skillDataManager.SkillConfigList[m_SkillIndex];
            EditorHook.editorSkillDisplayManager.SetSkill(CurSkill.Id);
            FreshSelfData();
            m_CurActionIndex = 0;
            m_ActionFrameOffset = 0;
        }

        private void InitWeaponData()
        {

        }

        private void Update()
        {
            if(EditorHook.editorSkillDisplayManager.IsPlay && m_TotalFrame>0)
            {
                Repaint();
            }
        }

        private void OnFocus()
        {
            EditorHook.editorSkillDisplayManager.MultipyFrame = 1;
        }

        private void FreshIDList()
        {
            m_SkillIDList.Clear();
            if(EditorHook.skillDataManager.SkillConfigList != null)
            {
                foreach(var item in EditorHook.skillDataManager.SkillConfigList)
                {
                    m_SkillIDList.Add(item.Id.ToString() + item.SkillName);
                }
            }
        }

        private void ResetData(bool changeIndex)
        {
            if (changeIndex)
                m_SkillIndex = 0;
            m_CurActionIndex = 0;
            m_ActionFrameOffset = 0;
            EditorHook.editorSkillDisplayManager.FirstFrame();
        }

        public void Refresh()
        {
            SetSkill(0, true);
            FreshIDList();
            FreshSelfData();
            Stop(false);
            Repaint();
        }

        private void FreshSelfData()
        {
            if (CurSkill == null) return;
            m_TotalFrame = EditorHook.skillDataManager.GetSkillTotleFrame(CurSkill);
            m_TotalDamagePerecent = EditorHook.skillDataManager.GetSkillTotalDamage(CurSkill);
        }

        private void OnGUI()
        {
            fx = 0;
            fy = 0;
            if (CurSkill == null)
            {
                DrawNewBtn();
                return;
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.HelpBox("选中此窗口以播放连续动作", MessageType.Info);
                    GUILineOne();
                    DrawSkillLine();
                    SkillRangeShow.Instance.IsShowRange = GUILayout.Toggle(SkillRangeShow.Instance.IsShowRange, "是否显示技能伤害范围");
                    DrawOptionButtonLine();

                    DrawFrameGrid();
                    GUILayout.Space(20);
                    DrawSkillAttr();
                    GUILayout.Space(10);
                }
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
            GUILayout.EndHorizontal();
            ProcessEventControl();
        }

        private void GUILineOne()
        {
            SkillEditorUtils.BeginContents();
            GUILayout.BeginHorizontal();
            { 
                if(EditorHook.skillDataManager.ShowName != null && EditorHook.skillDataManager.ShowName != string.Empty)
                {
                    GUILayout.Label("当前职业:" + EditorHook.skillDataManager.ShowName,GUILayout.Width(300));
                }
                GUILayout.Label("技能:",GUILayout.Width(50));
                SkillIndex = EditorGUILayout.Popup(SkillIndex,m_SkillIDList.ToArray(),GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            SkillEditorUtils.EndContents();
        }

        private void DrawSkillAttr()
        {
            if (CurSkill == null) return;
            fx = 0;
            fy += 40;

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            {
                AttrGUIHelper.GUI_Draw(CurSkill,"SkillData");
            }
            EditorGUILayout.EndScrollView();
        }

        public static int ShowID(string des,int id,int space)
        {
            id = AttrGUIHelper.GUI_Int_Attr<skillnew_data>(des,id,space);
            if(id != Instance.CurSkill.Id)
            {
                Instance.CurSkill.Id = id;
                Instance.IdRepeated = EditorHook.configCtrl.CheckIDRepeated<skillnew_data>(Instance.CurSkill.Id,"Id");
            }
            if(Instance.IdRepeated)
            {
                Color defcolor = GUI.contentColor;
                GUI.contentColor = Color.red;
                GUILayout.Label("this id is already exist");
                GUI.contentColor = defcolor;
            }
            return id;
        }

        #region 整体绘制
        //技能Id行
        private void DrawSkillLine()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("总帧数:"+m_TotalFrame,EditorStyles.label,GUILayout.Width(100));
                GUILayout.Space(20);
                GUILayout.Label("Speed:", GUILayout.Width(50));
                EditorHook.editorSkillDisplayManager.SetSpeed(EditorGUILayout.Slider(EditorHook.editorSkillDisplayManager.ActionSpeed,0.01f,2f,GUILayout.Width(200)));
                GUILayout.Space(20);
                GUILayout.Label(string.Format("CurrentTime : {0}",EditorHook.editorSkillDisplayManager.CurrentTime * 1000),GUILayout.Width(100));
                GUILayout.Space(20);
                GUILayout.Label(string.Format("总伤害百分比 : {0}",m_TotalDamagePerecent.ToString()),GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
        }

        //按钮行
        private void DrawOptionButtonLine()
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button(InnerIcon.FirstKey,GUILayout.Width(40),GUILayout.Height(40)))
            {
                Stop(false);
            }
            Texture texture = EditorHook.editorSkillDisplayManager.IsPlay ? InnerIcon.Pause : InnerIcon.Play;

            if (GUILayout.Button(texture,GUILayout.Width(40),GUILayout.Height(40)))
            {
                if (!EditorHook.editorSkillDisplayManager.IsPlay)
                    EditorHook.editorSkillDisplayManager.Play();
                else
                    EditorHook.editorSkillDisplayManager.Pause();
            }
            DrawNewBtn();
            DrawDeleteBtn();
            DrawSaveBtn();

            DrawReloadBtn();
            DrawFreshBtn();
            DrawResetFrameDataBtn();

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region DrawOptionButtonLine
        private void DrawResetFrameDataBtn()
        {
            if(GUILayout.Button("重置帧数",GUILayout.Width(100),GUILayout.Height(40)))
            {
                if (CurSkill == null) return;
                foreach(var item in CurSkill.SkillActionList)
                {
                    string name = EditorConfigUtil.GetAnimNameByPath(item.ActionPath);
                    if (EditorHook.editorModelCtrl[name] != null)
                        item.ActionTime = (int)(EditorHook.editorModelCtrl[name].length * 1000);
                }
                FreshSelfData();
            }
        }

        private void DrawFreshBtn()
        {
            if(GUILayout.Button("刷新",GUILayout.Width(100),GUILayout.Height(40)))
            {
                Refresh();
            }
        }

        private void DrawDeleteBtn()
        {
            if(GUILayout.Button("删除",GUILayout.Width(100),GUILayout.Height(40)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("deleta"), false, () =>
                {
                    EditorHook.skillDataManager.DeleteSkill(SkillIndex);
                    Refresh();

                    if (SkillIndex < 0 || SkillIndex >= m_SkillIDList.Count)
                        SkillIndex = 0;
                });
                menu.ShowAsContext();
            }
        }

        private void DrawNewBtn()
        {
            if(GUILayout.Button("新建",GUILayout.Width(100),GUILayout.Height(40)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("新建"), false, () => 
                {
                    EditorHook.skillDataManager.CreateNewSkill();
                    Refresh();

                    SkillIndex = -1;
                    SkillIndex = m_SkillIDList.Count - 1;
                    if (SkillIndex < 0 || SkillIndex >= m_SkillIDList.Count)
                        SkillIndex = 0;
                });

                menu.AddItem(new GUIContent("添加"), false, () =>
                {
                    EditorSkillSearchWindow window = GetWindow<EditorSkillSearchWindow>();
                    window.callBack = (skill) =>
                    {
                        EditorHook.skillDataManager.AddExistSkill(skill);
                        Refresh();

                        SkillIndex = m_SkillIDList.Count - 1;
                        if (SkillIndex < 0 || SkillIndex >= m_SkillIDList.Count)
                            SkillIndex = 0;
                    };
                    window.Show();
                });
                menu.ShowAsContext();
            }
        }


        private void DrawSaveBtn()
        {
            if(GUILayout.Button("保存当前技能",GUILayout.Width(100),GUILayout.Height(40)))
            {
                if(CurSkill !=null)
                {
                    if(CurSkill.SkillActionList != null)
                    {
                        foreach(var item in CurSkill.SkillActionList)
                        {
                            if(item.FrameEvents != null)
                            {
                                List<FrameEvent> frameEvents = new List<FrameEvent>();
                                for(int i=0;i<item.FrameEvents.Count;i++)
                                {
                                    frameEvents.Add(item.FrameEvents[i]);
                                }
                                frameEvents.Sort((a, b) => 
                                {
                                    return a.EventTimePoint - b.EventTimePoint;
                                });

                                item.FrameEvents.Clear();
                                item.FrameEvents.AddRange(frameEvents.ToArray());
                            }
                        }
                    }
                }
                EditorHook.skillDataManager.SavePartSkill();
            }
        }

        private void DrawReloadBtn()
        {
            if(GUILayout.Button("重新加载",GUILayout.Width(100),GUILayout.Height(40)))
            {
                EditorHook.configCtrl.LoadSkillConfig(EditorHook.skillDataManager.CurUnitConfig.Id);
                FreshIDList();
                FreshSelfData();
                Stop(false);
                int lastIndex = SkillIndex;
                SkillIndex = -1;
                SkillIndex = lastIndex;

                Repaint();
            }
        }
        #endregion


        #region DrawFrameGrid
        private void DrawFrameGrid()
        {
            m_FrameScroll = EditorGUILayout.BeginScrollView(m_Scroll, GUILayout.Height(300));
            fy += 80;
            GUI_Body_FrameNumber();
            GUI_Body_Grid();

            GUI_ScrollWindow();

            GUI_Body_FrameLine();
            GUI_Event();
            EditorGUILayout.EndScrollView();
            fy += DEFAULT_EVENT_HEIGHT * MAX_ORDER;
        }

        private void GUI_Body_FrameNumber()
        {
            fx = MARGIN_X;
            GUIStyle fontStyle = new GUIStyle();
            fontStyle.fontSize = 8;
            fontStyle.alignment = TextAnchor.LowerLeft;
            fontStyle.fontStyle = FontStyle.Normal;
            fontStyle.normal.textColor = Color.white;

            GUIStyle fontStyle2= new GUIStyle();
            fontStyle2.fontSize = 8;
            fontStyle2.alignment = TextAnchor.LowerLeft;
            fontStyle2.fontStyle = FontStyle.Normal;
            fontStyle2.normal.textColor = Color.yellow;

            Rect rect = new Rect();
            int nFrameCount = m_TotalFrame;
            for(int i= 0;i<nFrameCount;i++)
            {
                if(i% VERTICAL_GRID_LINE!=0)
                {
                    continue;
                }

                float fFrameX = fx + (i * DEFAULT_EVENT_WIDTH);
                rect.Set(fFrameX, fy, 30, FRAME_NUMBERS_HEIGHT);

                GUI.Label(rect, i.ToString(),i %(VERTICAL_GRID_LINE*2)!= 0? fontStyle:fontStyle2);
            }

            fy += FRAME_NUMBERS_HEIGHT;
        }

        private void GUI_Body_Grid()
        {
            fy += V_SPACE;
            //BG
            Color guiColor = GUI.color;
            Color backgroundColor = GUI.backgroundColor;

            Color lineColor = new Color(0.3f,0.3f,0.3f);
            Vector2 vPointA = Vector2.zero;
            Vector2 vPointB = Vector2.zero;

            if(EditorHook.skillDataManager.CurSkillConfig !=null)
            {
                foreach(var item in EditorHook.skillDataManager.CurSkillConfig.SkillActionList)
                {
                    int framenum = (int)EditorConfigUtil.ConvertTime2Frame(item.ActionTime);
                    float width = DEFAULT_EVENT_WIDTH * framenum;
                    Rect rect = new Rect(fx, fy, width, ACTION_HEIGHT);
                    fy += ACTION_HEIGHT;
                    fx += width;

                    if(GUI.Button(rect,EditorConfigUtil.GetAnimNameByPath(item.ActionPath)))
                    {
                        //右键菜单
                        if(Event.current.button ==1)
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("复制"), false, OnClickCopyAction, item);
                            menu.AddItem(new GUIContent("添加事件"), false, OnClickAddFrameEvent, item);
                            menu.AddItem(new GUIContent("删除"), false, OnClickDelete, item);

                            List<string> clips = EditorHook.editorModelCtrl.CollectSkillPath();
                            if(clips == null || clips.Count<=0)
                            {
                                menu.AddDisabledItem(new GUIContent("添加动画"));
                            }
                            else
                            {
                                //添加技能动作
                                foreach(var clipname in clips)
                                {
                                    menu.AddItem(new GUIContent("添加动画/"+clipname),false,(obj)=> 
                                    {
                                        OnClickAddAnim(obj as SkillActionInfo, clipname, EditorHook.editorModelCtrl.skillpath);
                                    },null);
                                }
                            }
                            menu.ShowAsContext();
                            Event.current.Use();
                        }
                        else
                        {
                            //展示动画
                            EditorHook.editorSkillDisplayManager.SetCurrentAction(item, CurSkill);
                            SkillEditorWindowMenu.OpenActionSequence();
                            Event.current.Use();
                        }
                    }
                }
            }
            GUI.color = guiColor;
            GUI.backgroundColor = backgroundColor;
            //Horizontal
            for(int i=0;i<=MAX_ORDER;i++)
            {
                float fTempY = fy + (i* DEFAULT_EVENT_HEIGHT);
                vPointA.Set(MARGIN_X,fTempY);
                vPointB.Set(MARGIN_X +(DEFAULT_EVENT_WIDTH * m_TotalFrame),fTempY);

                SkillEditorUtils.DrawLineVH(vPointA,vPointB,lineColor);
            }

            //Vertical
            Color lineColor2 = new Color(0.5f,0.5f,0.5f);

            for(int i=0;i<=m_TotalFrame;i++)
            {
                Color color;
                if((i%VERTICAL_GRID_LINE) != 0)
                {
                    color = lineColor;
                }
                else
                {
                    color = lineColor2;
                }
                //标记下动作有效起始帧
                if (i == EditorHook.editorSkillDisplayManager.StartFrame && i != 0)
                    color = new Color(Color.green.r,Color.green.g,Color.green.b,0.3f);

                float fTempX = MARGIN_X + (i * DEFAULT_EVENT_WIDTH);
                vPointA.Set(fTempX,fy);
                vPointB.Set(fTempX,fy +(DEFAULT_EVENT_HEIGHT * MAX_ORDER));

                SkillEditorUtils.DrawLineVH(vPointA,vPointB,color);
            }
        }


        private void GUI_ScrollWindow()
        {
            if (CurSkill == null || CurSkill.SkillActionList == null) return;
            //在每个窗口里把margin值减掉2，grid使用margin一般30 所以(30-2)* 2= 56

            int nScrollWidth = (DEFAULT_EVENT_WIDTH * m_TotalFrame) + ((MARGIN_X-2)*2);

            int actioCount = CurSkill.SkillActionList == null ? 0 : CurSkill.SkillActionList.Count;
            //只把Body部分的高度视为领域
            int nScrollHeigth = (DEFAULT_EVENT_HEIGHT * MAX_ORDER) + (FRAME_NUMBERS_HEIGHT * 2) + (FRAME_SLIDER_HEIGHT * 2) + actioCount * ACTION_HEIGHT;
            EditorGUILayout.BeginVertical();
            {
                Color oldColor = GUI.color;
                GUI.color = Color.black;

                GUILayout.Space(TOOLBAR_HEIGHT+15);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(2);
                    GUILayout.Box("",GUILayout.Width(nScrollWidth),GUILayout.Height(nScrollHeigth));
                }

                EditorGUILayout.EndHorizontal();

                GUI.color = oldColor;
            }
            EditorGUILayout.EndVertical();
        }

        private void GUI_Body_FrameLine()
        {
            float fLinex = MARGIN_X + CurrentFrame * DEFAULT_EVENT_WIDTH;

            Vector2 vPointA = new Vector2(fLinex,fy);
            Vector2 vPointB = new Vector2(fLinex, fy + (DEFAULT_EVENT_HEIGHT * MAX_ORDER));

            SkillEditorUtils.DrawLineVH(vPointA,vPointB,Color.red);
        }

        private void OnClickDelete(object item)
        {
            foreach(var action in CurSkill.SkillActionList)
            {
                if(action == item as SkillActionInfo)
                {
                    CurSkill.SkillActionList.Remove(action);
                    break;
                }
            }
            FreshSelfData();
        }    

        private void OnClickAddAnim(SkillActionInfo item,string clipname,string path)
        {
            int index = CurSkill.SkillActionList.Count;
            if(item != null)
            {
                for(int i=0;i<CurSkill.SkillActionList.Count;i++)
                {
                    if(item == CurSkill.SkillActionList[i])
                    {
                        index = i;
                        break;
                    }
                }
            }
            SkillActionInfo actionInfo = new SkillActionInfo();
            string animPath = ResourcePath.AddressableRoot + path + clipname;
            Hooks.ResourceManager.LoadAssetAsync<AnimationClip>(animPath, (clipload) =>
            {
                if(clipload != null)
                {
                    actionInfo.ActionTime = (int)(clipload.length * 1000);
                    actionInfo.ActionPath = path + clipload.name;
                }
                else
                {
                    Debug.LogError("load anim failed:"+path+clipname);
                }

                CurSkill.SkillActionList.Insert(index,actionInfo);

                FreshSelfData();

                EditorHook.editorSkillDisplayManager.SetSkill(CurSkill.Id);
            });
        }

        private void OnClickAddFrameEvent(object item)
        {
            foreach(var action in CurSkill.SkillActionList)
            {
                if(action == item as SkillActionInfo)
                {
                    action.FrameEvents.Add(new FrameEvent());
                    break;
                }
            }
        }

        private void OnClickCopyAction(object item)
        {
            m_Action4Copy = item as SkillActionInfo;
        }
        #endregion

        //处理鼠标事件
        private void ProcessEventControl()
        {
            Vector2 vMousePosition = m_Scroll + Event.current.mousePosition;
            if(Event.current.isMouse && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                GenericMenu menu = new GenericMenu();

                List<string> clips = EditorHook.editorModelCtrl.CollectSkillPath();
                if(clips == null || clips.Count<=0)
                {
                    menu.AddDisabledItem(new GUIContent("添加动画"));
                }
                else
                {
                    foreach(var clipname in clips)
                    {
                        menu.AddItem(new GUIContent("添加动画/" + clipname), false, (obj) => 
                        {
                            OnClickAddAnim(obj as SkillActionInfo,clipname,EditorHook.editorModelCtrl.skillpath);
                        },null);
                    }
                }

                if(m_Action4Copy != null && CurSkill != null)
                {
                    menu.AddItem(new GUIContent("粘贴"),false,()=> 
                    {
                        SkillActionInfo info = new SkillActionInfo();
                        info = GameFrame.DeepCopyHelper.DeepCopy(m_Action4Copy);

                        CurSkill.SkillActionList.Add(info);
                        FreshSelfData();
                    });
                }

                menu.ShowAsContext();
            }
        }

        #region //事件的绘制

        Dictionary<int, int> frameEventCountDict = new Dictionary<int, int>();

        private void GUI_Event()
        {
            if (CurSkill == null) return;

            if (CurSkill.SkillActionList == null || CurSkill.SkillActionList.Count <= 0) return;

            frameEventCountDict.Clear();

            int framepast = 0;
            int eventNo = 0;

            foreach(var item in CurSkill.SkillActionList)
            {
                RepeatedField<FrameEvent> events = item.FrameEvents;

                if (events == null || events.Count <= 0) continue;

                foreach(var e in events)
                {
                    int frame = (int)EditorConfigUtil.ConvertTime2Frame(e.EventTimePoint);
                    if(!frameEventCountDict.ContainsKey(frame))
                    {
                        frameEventCountDict.Add(frame,0);
                    }
                    else
                    {
                        frameEventCountDict[frame]++;
                    }

                    eventNo++;
                    GUI_Body_DrawEvent(framepast +frame,frameEventCountDict[frame],eventNo,e,events);
                }

                framepast += (int)EditorConfigUtil.ConvertTime2Frame(item.ActionTime);

            }
        }

        private void GUI_Body_DrawEvent(int startFrame,int yOffset,int eventsNo,FrameEvent data,RepeatedField<FrameEvent> events)
        {
            if (startFrame > m_TotalFrame)
                startFrame = m_TotalFrame;
            float eventX = MARGIN_X + ((startFrame)* DEFAULT_EVENT_WIDTH);
            float eventY = fy + yOffset * DEFAULT_EVENT_HEIGHT;
            DrawEvent(eventX,eventY, eventsNo, data,events);
        }

        private void DrawEvent(float fX,float fY,int eventNo,FrameEvent data,RepeatedField<FrameEvent> events)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.clipping = TextClipping.Overflow;
            labelStyle.contentOffset = new Vector3(-1,0,0);
            labelStyle.fontStyle = FontStyle.Bold;

            Rect rtPos = new Rect(fX,fY,DEFAULT_EVENT_WIDTH,DEFAULT_EVENT_HEIGHT);
            string name = "E" + eventNo;

            if(GUI.Button(rtPos,name,labelStyle))
            {
                if(Event.current.button ==1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("delete"), false, () => 
                    {
                        events.Remove(data);
                    });
                    menu.ShowAsContext();
                }
                else
                {
                    Debug.Log(data.FrameEventId);
                    SkillEditorWindowMenu.OpenFrameEventWindow(data,CurSkill);
                }
            }
        }
        #endregion

        //技能结束回调
        private void OnLastEnd()
        {
            Debug.Log("OnLastEnd");
            if (CurSkill == null)
                return;
            if (CurSkill.SkillActionList == null || CurSkill.SkillActionList.Count <= 0)
                return;

            m_CurActionIndex++;
            if(m_CurActionIndex >= CurSkill.SkillActionList.Count)
            {
                m_CurActionIndex = 0;
                m_ActionFrameOffset = 0;
                EditorHook.editorSkillDisplayManager.OnActionListOver();
            }
            else
            {
                m_ActionFrameOffset += (int)EditorConfigUtil.ConvertTime2Frame(CurSkill.SkillActionList[m_CurActionIndex-1].ActionTime);
            }

            if(EditorHook.editorSkillDisplayManager.ActionInfo != CurSkill.SkillActionList[m_CurActionIndex])
            {

            }
        }

        private void Stop(bool changeIndex)
        {
            ResetData(changeIndex);
            EditorHook.editorSkillDisplayManager.Stop();
        }
    }
}
