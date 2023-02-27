using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SkillnewConfig;
using System;

namespace GameEditor
{
    public enum eTOOLBAR_TEXTURE_TYPE
    {
        PLAY,
        PAUSE,
       //STOP,
       FIRST_FRAME,
       LAST_FRAME,
       PREV_FRAME,
       NEXT_FRAME,
       DELETE,
       DELETE_ALL,
       MaxCount,
    }

    public class ActionSequence : EditorWindow
    {
        public struct stCreateEventInfo
        {
            public int nStartFrame;
            public EFrameEventType eEventType;
        }

        public static ActionSequence Instance = null;

        #region Constant Variables
        public const int MARGIN_X = 30;
        public const int MARGIN_Y = 10;
        public const int MAX_ORDER = 10;
        public const int DEFAULT_EVENT_WIDTH = 10;
        public const int DEFAULT_EVENT_HEIGHT = 20;
        public const int TOOLBAR_CONTROL_MARGIN = 5;
        public const int TOOLBAR_HEIGHT = 50;
        public const int TOOLBAR_SPEED_WIDTH = 50;
        public const int INFOBAR_HEIGHT = 20;
        public const int INFOBAR_MARGIN = 5;
        public const int CONTROLBAR_MARGIN = 5;
        public const int CONTROLBAR_HEIGHT = 20;
        public const int FRAME_NUMBERS_HEIGHT = 12;
        public const int FRAME_SLIDER_HEIGHT = 17;//Player Slider 高度
        public const int VERTICAL_GRID_LINE = 5;//grid区分线在每几个Frame显示
        #endregion

        private static int m_nEventWidth = DEFAULT_EVENT_WIDTH;
        private static int m_nEventHeight = DEFAULT_EVENT_HEIGHT;
        private static int m_PlayTimeHeight = 0;//标示 play grid Frame number;

        private bool m_showSuperDefence = false;
        private Texture[] m_UITexture = new Texture[(int)eTOOLBAR_TEXTURE_TYPE.MaxCount];//工具栏里使用的texture
        private Vector2 m_vBodyScroll = Vector2.zero;
        private Vector2 m_vAttrScroll = Vector2.zero;
        private int m_nSelectIndex = -1;
        private Vector2 m_vCopyPos = Vector2.zero;
        private Rect m_DrawRect = new Rect();
        private Vector2 scrollViewPos = Vector2.zero;
        private bool m_IsFilterEventType = false;
        private List<EFrameEventType> filterTypeList = new List<EFrameEventType>() 
        {
            EFrameEventType.Effect,EFrameEventType.Sound,
        };

        private float heightOffset = 0;
        private Rect toolBarRect = new Rect(0,0,600,TOOLBAR_HEIGHT);
        private Rect ctrlBarRect = new Rect(0,TOOLBAR_HEIGHT,600,TOOLBAR_HEIGHT);
        private Rect colorRect = new Rect(0,TOOLBAR_HEIGHT*2+ 20,1000,TOOLBAR_HEIGHT *4);
        private Rect colorHeadRect = new Rect(0,TOOLBAR_HEIGHT * 2-10,600,30);
        private Rect bodyArea = new Rect();
        private Dictionary<FrameEvent, Rect> m_EventRectDict = new Dictionary<FrameEvent, Rect>();

        private int MultipyFrame { get { return EditorHook.editorSkillDisplayManager.ActionTotalFrame * EditorHook.editorSkillDisplayManager.MultipyFrame; } }

        private static FrameEvent m_CopyFrameEvent = null;

        public void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent("Sequence","d_CustonTool");
            position = new Rect(position.x,position.y,600,600);
            Instance = this;
            Initialize();
        }

        private void Initialize()
        {
            m_nSelectIndex = -1;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PLAY] = InnerIcon.Play;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PAUSE] = InnerIcon.Pause;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.LAST_FRAME] = InnerIcon.LastKey;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.FIRST_FRAME] = InnerIcon.FirstKey;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PREV_FRAME] = InnerIcon.PrevKey;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.NEXT_FRAME] = InnerIcon.NextKey;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.DELETE] = InnerIcon.Trash;
            m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.DELETE_ALL] = InnerIcon.Trash;

            SceneView.RepaintAll();
            m_nEventWidth = DEFAULT_EVENT_WIDTH;
            m_nEventHeight = DEFAULT_EVENT_HEIGHT;
        }

        public void OnDisable()
        {

        }

        private bool IsZoomOut()
        {
            return (m_nEventWidth!= DEFAULT_EVENT_WIDTH)|| (m_nEventHeight != DEFAULT_EVENT_HEIGHT);
        }

        private void OnFocus()
        {
            EditorHook.editorSkillDisplayManager.SingleActionMode = true;
            EditorHook.editorSkillDisplayManager.ActionEndCall = null;
        }

        private void OnLostFocus()
        {
            EditorHook.editorSkillDisplayManager.SingleActionMode = true;
        }

        private void Update()
        {
            if (EditorHook.editorSkillDisplayManager.IsPlay)
                Repaint();
        }
        #region 画Tool
        private void OnGUI()
        {
            ProcessEventControl();
            ProcessHotKey();

            GUI_ToolBar();
            GUI_ControlBar();
            GUILayout.BeginVertical(GUILayout.Width(600));

            GUILayout.BeginArea(colorHeadRect);
            GUILayout.Space(5);
            bool header = SkillEditorUtils.DrawHeader("事件颜色说明","EventColorHelp",false);

            GUILayout.Space(5);
            GUILayout.EndArea();
            GUILayout.EndVertical();

            if(header)
            {
                heightOffset = 280;
                GUI_EventColor();
            }
            else
            {
                heightOffset = 120;
            }

            int nScrollWidth = (m_nEventWidth * MultipyFrame) + ((MARGIN_X-2)* 2);
            int nScrollHeigth = (m_nEventHeight * MAX_ORDER) + (FRAME_NUMBERS_HEIGHT * 2) + (FRAME_SLIDER_HEIGHT *2);

            GUILayout.BeginVertical(GUILayout.Width(nScrollWidth>600 ? nScrollWidth:600));

            GUILayout.Space(heightOffset);
            m_vBodyScroll = GUILayout.BeginScrollView(m_vBodyScroll);
            {
                float fY = 0.0f;
                GUI_Body(ref fY);
                Color prevColor = Color.black;
                GUI.color = Color.black;
                GUI.Box(new Rect(10,-10,nScrollWidth,nScrollHeigth),"");
                GUI.color = prevColor;
                GUILayout.Space(nScrollHeigth);
            }
            GUILayout.EndScrollView();
            m_vAttrScroll = GUILayout.BeginScrollView(m_vAttrScroll);
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    GUI_ActionInfos();
                    GUILayout.Space(5);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// GUI_ActionTime 反射调用
        /// </summary>
        /// <param name="des"></param>
        /// <param name="time"></param>
        /// <param name="space"></param>
        /// <returns></returns>

        private static int GUI_Frame(string des,int time,int space)
        {
            time = EditorConfigUtil.ConvertFrame2Time(AttrGUIHelper.GUI_Int_Attr<SkillActionInfo>(des,EditorConfigUtil.ConvertTime2Frame(time),space));
            return time;
        }

        private void GUI_ActionInfos()
        {
            if(EditorHook.editorSkillDisplayManager.ActionInfo != null)
            {
                //使用反射获取FrameEvent属性将其绘制
                GUILayout.BeginVertical();
                AttrGUIHelper.GUI_Draw(EditorHook.editorSkillDisplayManager.ActionInfo,"SkillActionInfo");
                GUILayout.EndVertical();
            }
        }

        private void GUI_ToolBar()
        {
            int nButtonSize = TOOLBAR_HEIGHT - (TOOLBAR_CONTROL_MARGIN * 2);
            GUILayout.BeginArea(toolBarRect, EditorStyles.toolbar.normal.background);
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            //first frame
            if (GUILayout.Button(InnerIcon.FirstKey, GUILayout.Width(nButtonSize), GUILayout.Height(nButtonSize)))
            {
                EditorHook.editorSkillDisplayManager.FirstFrame();
                EditorHook.editorSkillDisplayManager.Stop();
            }
            //prev frame
            if(GUILayout.Button(m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PREV_FRAME],GUILayout.Width(nButtonSize)))
            {
                EditorHook.editorSkillDisplayManager.PrevFrame();
            }
            //play && pause
            Texture playToggleTexture = EditorHook.editorSkillDisplayManager.IsPlay ? m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PAUSE] :
                m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.PLAY];
            if(GUILayout.Button(playToggleTexture,GUILayout.Width(nButtonSize),GUILayout.Height(nButtonSize)))
            {
                EditorHook.editorSkillDisplayManager.Stop();
                if (!EditorHook.editorSkillDisplayManager.IsPlay)
                {
                    EditorHook.editorSkillDisplayManager.SetTime(EditorHook.editorSkillDisplayManager.StartTime);
                    EditorHook.editorSkillDisplayManager.Play();
                }
                else
                    EditorHook.editorSkillDisplayManager.Pause();
            }

            //next frame
            if(GUILayout.Button(m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.NEXT_FRAME],GUILayout.Width(nButtonSize),GUILayout.Height(nButtonSize)))
            {
                EditorHook.editorSkillDisplayManager.NextFrame();
            }
            //last frame
            if(GUILayout.Button(m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.LAST_FRAME],GUILayout.Width(nButtonSize),GUILayout.Height(nButtonSize)))
            {
                EditorHook.editorSkillDisplayManager.LastFrame();
            }
            //Speed
            float fValue = GUILayout.HorizontalSlider(EditorHook.editorSkillDisplayManager.ActionSpeed,0.01f,4.0f,GUILayout.Width(TOOLBAR_SPEED_WIDTH),GUILayout.Height(nButtonSize));
            if(fValue != EditorHook.editorSkillDisplayManager.ActionSpeed)
            {
                EditorHook.editorSkillDisplayManager.SetSpeed(fValue);
            }
            GUIStyle fontStyle = new GUIStyle("textfield");
            fontStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.TextField(EditorHook.editorSkillDisplayManager.ActionSpeed.ToString("N2"),fontStyle,GUILayout.Width(TOOLBAR_SPEED_WIDTH),GUILayout.Height(nButtonSize-20));
            GUI_ToolBar_Delete(nButtonSize);
            GUI_ToolBar_ZoomIn(nButtonSize);
            GUI_ToolBar_ZoomOut(nButtonSize);
            GUI_ToolBar_ZoomReset(nButtonSize);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void GUI_ToolBar_Delete(float nButtonSize)
        {
            if(m_nSelectIndex == -1)
            {
                GUI.enabled = false;
            }
            if(GUILayout.Button(m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.DELETE],GUILayout.Width(nButtonSize),GUILayout.Height(nButtonSize)))
            {
                if(EditorUtility.DisplayDialog("Action Tool","Remove current event","OK","CANCEL")== true)
                {
                    RemoveCurrentEvent();
                }
            }

            if (GUILayout.Button(new GUIContent("All",m_UITexture[(int)eTOOLBAR_TEXTURE_TYPE.DELETE_ALL]), GUILayout.Width(nButtonSize), GUILayout.Height(nButtonSize)))
            {
                if (EditorUtility.DisplayDialog("Action Tool", "Remove All events", "OK", "CANCEL") == true)
                {
                    RemoveAllEvent();
                }
            }

            if(GUI.enabled == false)
            {
                GUI.enabled = true;
            }
        }

        private void GUI_ToolBar_ZoomIn(int nSize)
        {

        }

        private void GUI_ToolBar_ZoomOut(int nSize)
        {

        }

        private void GUI_ToolBar_ZoomReset(int nSize)
        {

        }

        private void GUI_ControlBar()
        {
            GUILayout.BeginArea(ctrlBarRect,EditorStyles.toolbar.normal.background);
            GUILayout.BeginVertical();
            GUILayout.Space(TOOLBAR_CONTROL_MARGIN);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUIStyle style = new GUIStyle(GUI.skin.toggle);
            style.alignment = TextAnchor.MiddleLeft;
            EditorHook.editorSkillDisplayManager.IsSoundPlay = GUILayout.Toggle(EditorHook.editorSkillDisplayManager.IsSoundPlay, "Sound Play", GUILayout.Width(100), GUILayout.Height(CONTROLBAR_HEIGHT)) ;
            EditorHook.editorSkillDisplayManager.Loop = GUILayout.Toggle(EditorHook.editorSkillDisplayManager.Loop,"Loop",GUILayout.Width(100),GUILayout.Height(CONTROLBAR_HEIGHT));
            int multipy = EditorGUILayout.IntField("Franme multipy:",EditorHook.editorSkillDisplayManager.MultipyFrame,GUILayout.Width(170),GUILayout.Height(INFOBAR_HEIGHT));
            if(multipy<1)
            {
                multipy = 1;
            }
            EditorHook.editorSkillDisplayManager.MultipyFrame = multipy;

            m_IsFilterEventType = GUILayout.Toggle(m_IsFilterEventType, "过滤event(美术)", GUILayout.Width(200), GUILayout.Height(CONTROLBAR_HEIGHT));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void GUI_EventColor()
        {
            string[] names = Enum.GetNames(typeof(EFrameEventType));
            Color prevColor = GUI.backgroundColor;

            GUILayout.BeginArea(colorRect);

            int stepLen = names.Length / 4;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < names.Length; i++)
            {
                string item = names[i];
                GUILayout.BeginHorizontal(GUILayout.Width(m_nEventWidth + 100), GUILayout.Height(m_nEventHeight));
                GUI.backgroundColor = GetEventColor((EFrameEventType)i, prevColor);
                GUILayout.Label(item, GUI.skin.button, GUILayout.Width(m_nEventWidth), GUILayout.Height(m_nEventHeight));
                GUI.backgroundColor = prevColor;
                GUILayout.Label(item);
                GUILayout.EndHorizontal();
                if (i % stepLen == 0 && i != 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void GUI_Body(ref float fY)
        {
            float fX = MARGIN_X;
            int nGridWidth = m_nEventWidth * MultipyFrame;
            int nGridHeight = m_nEventHeight * MAX_ORDER;

            GUI_Body_FrameNumber(ref fX,ref fY,nGridWidth);
            GUI_Body_PlayBar(ref fX,ref fY,nGridWidth);
            GUI_Body_Grid(ref fX,ref fY,nGridWidth,nGridHeight);
            GUI_Body_Events(fX,fY);
            GUI_Body_FrameLine(ref fX,ref fY);

            ClearEventRectDict();
        }

        private void GUI_Body_FrameNumber(ref float fX,ref float fY,int nWidth)
        {
            GUIStyle fontStyle = new GUIStyle();
            fontStyle.fontSize = 8;
            fontStyle.alignment = TextAnchor.LowerLeft;
            fontStyle.fontStyle = FontStyle.Normal;
            fontStyle.normal.textColor = Color.white;

            GUIStyle fontStyle2 = new GUIStyle();
            fontStyle2.fontSize = 8;
            fontStyle2.alignment = TextAnchor.LowerLeft;
            fontStyle2.fontStyle = FontStyle.Normal;
            fontStyle2.normal.textColor = Color.yellow;

            Rect rect = new Rect();
            int nFrameCpunt = MultipyFrame;
            for(int i=0;i<nFrameCpunt;i++)
            {
                if(i% VERTICAL_GRID_LINE !=0)
                {
                    continue;
                }

                float fFrameX = fX + (i * m_nEventWidth);

                rect.Set(fFrameX,fY,30,FRAME_NUMBERS_HEIGHT);

                GUI.Label(rect,i.ToString(),i % (VERTICAL_GRID_LINE* 2)!=0? fontStyle:fontStyle2);
            }

            fY += FRAME_NUMBERS_HEIGHT;
        }
            

        private void GUI_Body_PlayBar(ref float fX,ref float fY,int nWidth)
        {
            float fSlotOffset = 4.0f;
            Rect rect = new Rect(fX - fSlotOffset,fY,nWidth+(fSlotOffset*2.0f),FRAME_SLIDER_HEIGHT);
            float curtime = GUI.HorizontalSlider(rect,EditorHook.editorSkillDisplayManager.ActionCurTime,0.0f,EditorHook.editorSkillDisplayManager.ActionLength);
            if(!Mathf.Approximately(EditorHook.editorSkillDisplayManager.ActionCurTime,curtime))
            {
                float delta = curtime - EditorHook.editorSkillDisplayManager.ActionCurTime;
                if(delta >0)
                {
                    int step = Mathf.CeilToInt(delta / EditorSkillDisplayManager.FRAME);
                    float value = delta / step;
                    for(int i=0;i<step;i++)
                    {
                        EditorHook.editorSkillDisplayManager.UpdateSkill(value);
                    }
                }
                else
                {
                    int step = Mathf.CeilToInt(-delta/EditorSkillDisplayManager.FRAME);
                    float value = delta / step;
                    for (int i = 0; i < step; i++)
                        EditorHook.editorSkillDisplayManager.UpdateSkill(value);
                }
            }

            fY += FRAME_SLIDER_HEIGHT;
        }

        private void GUI_Body_Grid(ref float fX,ref float fY,int nWidth,int nHeight)
        {
            //BG
            m_DrawRect.Set(MARGIN_X,fY + heightOffset,nWidth,nHeight);
            Rect rect = new Rect(MARGIN_X,fY,nWidth,nHeight);
            GUI.Box(rect,"");

            Color lineColor = new Color(0.3f,0.3f,0.3f);
            Vector2 vPointA = Vector2.zero;
            Vector2 vPointB = Vector2.zero;

            //Horizontal
            for(int i= 1;i<MAX_ORDER;i++)
            {
                float fTempY = fY + (i * m_nEventHeight);
                vPointA.Set(MARGIN_X,fTempY);
                vPointB.Set(MARGIN_X+(m_nEventWidth * MultipyFrame),fTempY);

                SkillEditorUtils.DrawLineVH(vPointA,vPointB,lineColor);
            }

            //Vertical
            Color lineColor2 = new Color(0.5f,0.5f,0.5f);
            int nTotalFrame = MultipyFrame;
            for(int i=1;i<nTotalFrame;i++)
            {
                Color color;
                if((i% VERTICAL_GRID_LINE)!= 0)
                {
                    color = lineColor;
                }
                else
                {
                    color = lineColor2;
                }

                //标记下动作有效的起始帧
                if (i == EditorHook.editorSkillDisplayManager.StartFrame && i != 0)
                    color = new Color(Color.green.r,Color.green.g,Color.green.b,0.3f);

                float fTempX = MARGIN_X + (i * m_nEventWidth);
                vPointA.Set(fTempX,fY);
                vPointB.Set(fTempX,fY +(m_nEventHeight *MAX_ORDER));

                SkillEditorUtils.DrawLineVH(vPointA, vPointB, color);
            }
        }

        Dictionary<int, int> dicOrder = new Dictionary<int, int>();

        private void GUI_Body_Events(float fX,float fY)
        {
            if (EditorHook.editorSkillDisplayManager.Events == null) return;
            dicOrder.Clear();
            FrameEvent nextEvt = null;
            for(int i=0;i<EditorHook.editorSkillDisplayManager.Events.Count;i++)
            {
                nextEvt = null;
                FrameEvent evt = EditorHook.editorSkillDisplayManager.Events[i];
                //过滤evnt(美术)
                bool drawGUI = true;
                if (m_IsFilterEventType && !filterTypeList.Contains(evt.EventType))
                    drawGUI = false;
                switch(evt.EventType)
                {
                    case EFrameEventType.ChangeDir:
                    case EFrameEventType.CameraFollowHeight:
                    case EFrameEventType.ChangeSuperDefence:
                    case EFrameEventType.PushTargets:
                        {
                            if(nextEvt ==null)
                            {
                                for(int j=i+1;j<EditorHook.editorSkillDisplayManager.Events.Count;j++)
                                {
                                    FrameEvent _findevt = EditorHook.editorSkillDisplayManager.Events[j];
                                    if(_findevt.EventType == evt.EventType)
                                    {
                                        nextEvt = _findevt;
                                        break;
                                    }
                                }
                                if(nextEvt !=null)
                                {
                                    int frameNum = EditorConfigUtil.ConvertTime2Frame(nextEvt.EventTimePoint - evt.EventTimePoint) + 1;
                                    if (frameNum < 1)
                                        frameNum = 1;
                                    DrawEvent(evt,fX,fY,i,drawGUI,ref dicOrder,frameNum,nextEvt);
                                }
                            }
                            else
                            {
                                nextEvt = null;
                            }
                            break;
                        }
                    case EFrameEventType.CameraBlur:
                    case EFrameEventType.CameraBlurLow:
                    case EFrameEventType.CameraBlurHigh:
                    case EFrameEventType.LockCameraRotate:
                    case EFrameEventType.CameraFollowWeaponX:
                    case EFrameEventType.CameraFollowWeaponY:
                        DrawEvent(evt,fX,fY,i,drawGUI,ref dicOrder,Math.Max(evt.FrameEventId,1));
                        break;
                    default:
                        DrawEvent(evt, fX, fY, i, drawGUI, ref dicOrder, 1);
                        break;
                }
            }
            if(m_vCopyPos!= Vector2.zero)
            {
                m_vCopyPos = Vector2.zero;
            }
        }

        private void GUI_Body_FrameLine(ref float fX, ref float fY)
        {
            float fLineX = MARGIN_X + (EditorHook.editorSkillDisplayManager.ActionCurFrame * m_nEventWidth);

            Vector2 vPointA = new Vector2(fLineX,fY);
            Vector2 vPointB = new Vector2(fLineX,fY+(m_nEventHeight * MAX_ORDER));

            SkillEditorUtils.DrawLineVH(vPointA,vPointB,Color.red);
        }

        private void DrawEvent(FrameEvent fevent,float fX,float fY,int nIndex,bool drawGUI,ref Dictionary<int,int> dicOrder,int frameLen =1,FrameEvent secondEvt = null)
        {
            int frame = (int)EditorConfigUtil.ConvertTime2Frame(fevent.EventTimePoint);
            if(!dicOrder.ContainsKey(frame))
            {
                dicOrder.Add(frame,0);
            }

            float fLeft = fX + (frame* m_nEventWidth);
            float fTop = fY+(dicOrder[frame]*m_nEventHeight);

            if(frameLen>1)
            {
                for(int i=0;i<frameLen;i++)
                {
                    if(!dicOrder.ContainsKey(frame+1))
                    {
                        dicOrder.Add(frame+i,dicOrder[frame]+1);
                    }
                    else
                    {
                        dicOrder[frame + i] = dicOrder[frame] + 1;
                    }
                }
            }

            dicOrder[frame]++;
            float width = m_nEventWidth * frameLen;
            Rect rtPos = new Rect(fLeft,fTop,width,m_nEventHeight);
            m_EventRectDict[fevent] = rtPos;
            if (!drawGUI)
                return;
            GUIStyle style = new GUIStyle(GUI.skin.button);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            Color prevColor = GUI.color;
            Color prevBgColor = GUI.backgroundColor;
            string strFullName = fevent.EventType.ToString() + " " + nIndex.ToString();
            GUI.color = Color.white;
            GUI.backgroundColor = GetEventColor(fevent.EventType,prevBgColor);
            if(GUI.Button(rtPos,"",style))
            {
                m_nSelectIndex = nIndex;
                if(secondEvt==null)
                {
                    SkillEditorWindowMenu.OpenFrameEventWindow(fevent,EditorHook.editorSkillDisplayManager.CurSkill);
                }
                else
                {
                    SkillEditorWindowMenu.OpenFrameEventWindow(fevent, EditorHook.editorSkillDisplayManager.CurSkill,secondEvt);
                }
            }

            int maxStrWidth = (int)(width * 0.1f);
            string labStr = strFullName.Length > maxStrWidth ? strFullName.Substring(0, maxStrWidth) : strFullName;

            if(m_nSelectIndex == nIndex)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.yellow;
                int strLenHalf = labStr.Length * 5;
                rtPos.Set(rtPos.x+width*0.5f-strLenHalf,rtPos.y,rtPos.width,rtPos.height);
            }
            GUI.Label(rtPos,labStr);
            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;
        }

        #endregion

        #region Event处理
        public void CreateEvent(object objCreateEventInfo)
        {
            stCreateEventInfo info = (stCreateEventInfo)objCreateEventInfo;

            EditorHook.editorSkillDisplayManager.CreateEvent(info.nStartFrame,info.eEventType);
            if(info.eEventType == EFrameEventType.ChangeDir || info.eEventType == EFrameEventType.ChangeSuperDefence || info.eEventType == EFrameEventType.CameraFollowHeight)
            {
                EditorHook.editorSkillDisplayManager.CreateEvent(info.nStartFrame +1,info.eEventType);
            }
            m_nSelectIndex = EditorHook.editorSkillDisplayManager.Events.Count - 1;
            Repaint();
        }


        public void RemoveCurrentEvent()
        {
            if(m_nSelectIndex == -1)
            {
                return;
            }
            EditorHook.editorSkillDisplayManager.DeleteEvent(m_nSelectIndex);
        }

        public void RemoveAllEvent()
        {
            EditorHook.editorSkillDisplayManager.DeleteAllEvent();
            m_nSelectIndex = -1;
        }

        /// <summary>
        /// 根据pos获取event的index
        /// </summary>
        /// <param name="pos"></param>
        private int GetEventIndexByPos(Vector2 pos)
        {
            Vector2 realPos = new Vector2(pos.x,pos.y-heightOffset);
            var enumerator = m_EventRectDict.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if(enumerator.Current.Value.Contains(realPos))
                {
                    return EditorHook.editorSkillDisplayManager.Events.IndexOf(enumerator.Current.Key);
                }
            }
            return -1;
        }
        /// <summary>
        /// 清除失效的event
        /// </summary>
        private void ClearEventRectDict()
        {
            List<FrameEvent> tempList = new List<FrameEvent>();
            var enumerator = m_EventRectDict.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if (!EditorHook.editorSkillDisplayManager.Events.Contains(enumerator.Current.Key))
                    tempList.Add(enumerator.Current.Key);
            }

            foreach(FrameEvent fEvent in tempList)
            {
                m_EventRectDict.Remove(fEvent);
            }
        }

        #endregion


        #region 放大/缩小
        void ZoomInWidth(bool bPrecision)
        {
            if(bPrecision == true)
            {
                m_nEventWidth++;
            }
            else
            {
                m_nEventWidth += DEFAULT_EVENT_WIDTH;
            }

            Repaint();
        }

        void ZoomOutWidth(bool bPrecision)
        {
            if(bPrecision == true)
            {
                m_nEventWidth--;
                if(m_nEventWidth<DEFAULT_EVENT_WIDTH)
                {
                    m_nEventWidth = DEFAULT_EVENT_WIDTH;
                }
                else
                {
                    m_nEventWidth -= DEFAULT_EVENT_WIDTH;
                    if(m_nEventWidth < DEFAULT_EVENT_WIDTH)
                    {
                        m_nEventWidth = DEFAULT_EVENT_WIDTH;
                    }
                }
            }

            Repaint();
        }
        #endregion

        #region 键盘/鼠标处理
        private void ProcessEventControl()
        {
            float fToolBarHeight = TOOLBAR_HEIGHT + m_PlayTimeHeight + FRAME_SLIDER_HEIGHT;
            Vector2 vMousePosition = m_vBodyScroll + Event.current.mousePosition;
            if(Event.current.isMouse && Event.current.button ==0 && Event.current.type == EventType.MouseDown)
            {
                if(vMousePosition.y<fToolBarHeight)
                {
                    return;
                }
            }

            if(Event.current.isMouse == true && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                if (!m_DrawRect.Contains(vMousePosition))
                    return;
                int nStartFrame = (int)(vMousePosition.x - MARGIN_X) / m_nEventWidth;
                int nOrder = (int)(vMousePosition.y - fToolBarHeight) / m_nEventHeight;
                Debug.LogFormat("Start Frame:{0},Order:{1}",nStartFrame,nOrder);
                GenericMenu addTargetMenu = new GenericMenu();
                foreach(EFrameEventType type in Enum.GetValues(typeof(EFrameEventType)))
                {
                    //过滤event(美术)
                    if(m_IsFilterEventType ? filterTypeList.Contains(type):type != EFrameEventType.EventTypeNone)
                    {
                        stCreateEventInfo info;
                        info.nStartFrame = nStartFrame;
                        info.eEventType = type;
                        string strPopup = EditorHook.description.GetDescription<EFrameEventType>(type.ToString());
                        strPopup = string.Format("添加事件/{0}",strPopup.Replace("|","/"));
                        addTargetMenu.AddItem(new GUIContent(strPopup),false,CreateEvent,info);
                    }
                }

                addTargetMenu.AddItem(new GUIContent("复制"),false,CopyEvent,vMousePosition);
                addTargetMenu.AddItem(new GUIContent("粘贴"),false,PasteEvent, nStartFrame);
                addTargetMenu.AddItem(new GUIContent("编辑"),false,EditEvent);
                addTargetMenu.ShowAsContext();
                
            }
            //middle mouse key
            if (Event.current.isMouse && Event.current.button == 2)
            {
                if (!m_DrawRect.Contains(vMousePosition))
                    return;
                if (Event.current.type == EventType.MouseDown)
                {
                    int tempIndex = GetEventIndexByPos(Event.current.mousePosition);
                    if (tempIndex >= 0)
                        m_nSelectIndex = tempIndex;
                }

                if (m_nSelectIndex >= 0 && Event.current.type == EventType.MouseDrag)
                {
                    int nStartFrame = (int)(vMousePosition.x - MARGIN_X) / m_nEventWidth;
                    EditorHook.editorSkillDisplayManager.SetEventFrame(m_nSelectIndex, nStartFrame);
                }
            }
        }

        void ProcessHotKey()
        {
            //播放/终止
            if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Space)
            {
                EditorHook.editorSkillDisplayManager.Play();
                Debug.Log("Space : "+EditorHook.editorSkillDisplayManager.IsPlay.ToString());
            }

            if(EditorHook.editorSkillDisplayManager.IsPlay)
            {
                return;
            }

            if(focusedWindow != this)
            {
                return;
            }

            //调整Grid大小
            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow)
            {
                ZoomInWidth(Event.current.control);
            }
            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow)
            {
                ZoomOutWidth(Event.current.control);
            }

            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
            {
                if(Event.current.control == true)
                {
                    EditorHook.editorSkillDisplayManager.FirstFrame();
                }
                else if(Event.current.shift == true)
                {
                    EditorHook.editorSkillDisplayManager.PrevFrame(3);
                }
                else
                {
                    EditorHook.editorSkillDisplayManager.PrevFrame();
                }

                Repaint();
            }

            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
            {
                if (Event.current.control == true)
                {
                    EditorHook.editorSkillDisplayManager.LastFrame();
                }
                else if (Event.current.shift == true)
                {
                    EditorHook.editorSkillDisplayManager.NextFrame(3);
                }
                else
                {
                    EditorHook.editorSkillDisplayManager.NextFrame();
                }
                Repaint();
            }

            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                if(EditorUtility.DisplayDialog("Action Tool","Remove current event","OK","CANCEL") == true)
                {
                    RemoveCurrentEvent();
                }
            }
        }
        #endregion


        private void CopyEvent(object param)
        {
            m_vCopyPos = (Vector2)param;
            int tempIndex = GetEventIndexByPos(m_vCopyPos);
            if(tempIndex >=0)
            {
                m_CopyFrameEvent = GameFrame.DeepCopyHelper.DeepCopy(EditorHook.editorSkillDisplayManager.Events[tempIndex]);
            }
        }

        private void PasteEvent(object param)
        {
            if(m_CopyFrameEvent != null)
            {
                FrameEvent frameEvent = GameFrame.DeepCopyHelper.DeepCopy(m_CopyFrameEvent);
                frameEvent.EventTimePoint = EditorConfigUtil.ConvertFrame2Time((int)param);
                EditorHook.editorSkillDisplayManager.Events.Add(frameEvent);
                Repaint();
            }
        }

        private void OnDestroy()
        {
            FrameEventWindow window = GetWindow<FrameEventWindow>();
            window.Close();
        }

        private void EditEvent()
        {
            if(EditorHook.editorSkillDisplayManager.Events != null && m_nSelectIndex >= 0 && m_nSelectIndex<EditorHook.editorSkillDisplayManager.Events.Count)
            {
                SkillEditorWindowMenu.OpenFrameEventWindow(EditorHook.editorSkillDisplayManager.Events[m_nSelectIndex],EditorHook.editorSkillDisplayManager.CurSkill);
            }
        }

        private Color GetEventColor(EFrameEventType etype,Color defaultColor)
        {
            string[] names = Enum.GetNames(typeof(EFrameEventType));
            int max = names.Length / 3 + 1;

            int value = (int)etype / max;
            int mv = (int)etype % max;
            switch(value)
            {
                case 0:
                    return new Color(mv / (float)max, 0, 0, 1);
                case 1:
                    return new Color(0, mv / (float)max, 0, 1);
                case 2:
                    return new Color(0,0,mv/(float)max,1);
            }
            return defaultColor;
        }
    }
}
