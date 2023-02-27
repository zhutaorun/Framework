using System;
using GameFrame.SkillDisplay;
using UnityEngine;
using Google.Protobuf.Collections;
using SkillnewConfig;

namespace GameEditor
{
    /// <summary>
    /// 持有运行状态下的Skilldisplaymanager,扩展编辑器下的功能
    /// </summary>
    public class EditorSkillDisplayManager:IDisposable
    {
        public const float FPS = 30.0f;
        public const float FRAME = 1 / FPS;

        public Action ActionEndCall = null;
        public bool Loop = true;
        public int MultipyFrame = 1;

        public bool IsSoundPlay { get; set; }

        #region Sequence 变量
        private float m_StartTime;
        private float m_EndTime = 0;
        public float ActionTotalTime;
        public skillnew_data CurSkill;
        public SkillActionInfo ActionInfo;
        #endregion

        public float ActionSpeed { get { return SkillDisplayManager.Instance.TimeSpeed; } }
        public int StartFrame { get; set; }//当前有效开始帧数

        public float StartTime { get { return m_StartTime; } }

        public float ActionCurTime { get { return CurrentTime - StartTime; } }


        public int ActionCurFrame { get { return EditorConfigUtil.ConvertSecond2Frame(ActionCurTime); } }

        public float ActionLength { get { return ActionTotalTime * MultipyFrame; } }

        public int ActionTotalFrame { get { return EditorConfigUtil.ConvertSecond2Frame(ActionTotalTime); } }

        public bool SingleActionMode = false;

        public RepeatedField<FrameEvent> Events
        {
            get
            {
                if (ActionInfo == null) return null;
                return ActionInfo.FrameEvents;
            }
        }

        public void SetSpeed(float value)
        {
            SkillDisplayManager.Instance.TimeSpeed = value;
            Time.timeScale = value;
        }

        public T GetCtrl<T>() where T:class
        {
            return SkillDisplayManager.Instance.GetCtrl<T>();
        }


        public EditorSkillDisplayManager()
        {
            SkillDisplayManager.CreateInstance();
        }

        public void Dispose()
        {
            SkillDisplayManager.DestoryInstance();
        }


        public void SetCurrentAction(SkillActionInfo actionInfo,skillnew_data skill)
        {
            ActionInfo = actionInfo;
            ActionTotalTime = ActionInfo.ActionTime * 0.001f;
            CurSkill = skill;
            foreach(var action in skill.SkillActionList)
            {
                if(action == actionInfo)
                {
                    m_EndTime = m_StartTime + action.ActionTime;
                    break;
                }

                m_StartTime += action.ActionTime;
            }
            StartFrame = EditorConfigUtil.ConvertTime2Frame(actionInfo.ActionStartPoint);
            m_StartTime = ConfigUtil.ConvertTime((int)m_StartTime);
            m_EndTime = ConfigUtil.ConvertTime((int)m_EndTime);
        }

        public void OnActionListOver()
        {

        }

        public void OnCurrentActionOver()
        {

        }

        #region 按钮功能方法
        public void FirstFrame()
        {
            SetTime(m_StartTime);
        }

        public void PrevFrame(int frame = 1)
        {
            UpdateSkill(-FRAME* frame);
        }

        public void SetTime(float time)
        {
            SkillDisplayManager.Instance.SetToTime(time);
        }

        public void NextFrame(int frame = 1)
        {
            UpdateSkill(FRAME * frame);
        }

        public void LastFrame()
        {
            SetTime(m_EndTime);
        }
        #endregion
        #region PlayCtrl
        public void Play()
        {
            SkillDisplayManager.Instance.Play();
        }

        public void Pause()
        {
            SkillDisplayManager.Instance.Pause();
        }

        public void Stop()
        {
            SkillDisplayManager.Instance.Stop();
        }
        #endregion


        #region EventCtrl
        public void DeleteEvent(int index)
        {
            if (ActionInfo == null)
                return;
            if (ActionInfo.FrameEvents == null && ActionInfo.FrameEvents.Count <= index)
                return;
            ActionInfo.FrameEvents.RemoveAt(index);
        }

        public void DeleteAllEvent()
        {
            if (ActionInfo == null)
                return;
            if (ActionInfo.FrameEvents == null )
                return;
            ActionInfo.FrameEvents.Clear();
        }

        public FrameEvent CreateEvent(int startFrame,EFrameEventType eventType)
        {
            if (ActionInfo == null)
                return null;

            FrameEvent frameEvent = new FrameEvent();
            frameEvent.EventTimePoint = EditorConfigUtil.ConvertFrame2Time(startFrame);
            frameEvent.EventType = eventType;
            bool insert = false;
            for(int i=0;i<ActionInfo.FrameEvents.Count;i++)
            {
                if(frameEvent.EventTimePoint < ActionInfo.FrameEvents[i].EventTimePoint)
                {
                    ActionInfo.FrameEvents.Insert(i,frameEvent);
                    insert = true;
                    break;
                }
            }
            if (!insert)
                ActionInfo.FrameEvents.Add(frameEvent);
            return frameEvent;
        }

        public void SetEventFrame(int index,int frame)
        {
            if (ActionInfo == null)
                return;
            if (ActionInfo.FrameEvents == null && ActionInfo.FrameEvents.Count <= index)
                return;

            ActionInfo.FrameEvents[index].EventTimePoint = EditorConfigUtil.ConvertFrame2Time(frame);
        }
        #endregion

        #region SkillDisplayManager
        public int TotalFrameP { get { return EditorConfigUtil.ConvertSecond2Frame(SkillDisplayManager.Instance.SkillData.TotalTime); } }
        
        
        public bool IsPlay { get { return SkillDisplayManager.Instance.IsPlaying; } }

        public float CurrentTime { get { return SkillDisplayManager.Instance.CurrentTime; } }

        public int CurrentFrame { get { return EditorConfigUtil.ConvertSecond2Frame(SkillDisplayManager.Instance.CurrentTime); } }

        public void SetSkill(int id)
        {
            SkillDisplayManager.Instance.SetSkill(id);
        }

        public void TryUpdateSkill(float deltaTime)
        {
            CheckEnd();
            SkillDisplayManager.Instance.TryUpdateSkill(deltaTime* SkillDisplayManager.Instance.TimeSpeed);
        }

        public void UpdateSkill(float deltaTime)
        {
            CheckEnd();
            SkillDisplayManager.Instance.UpdateSkill(deltaTime);
        }

        private void CheckEnd()
        {
            if (!SingleActionMode) return;
            if(CurrentTime > m_EndTime)
            {
                SkillDisplayManager.Instance.SetToTime(m_StartTime);
            }
        }
        #endregion
    }

}