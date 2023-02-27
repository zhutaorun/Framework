using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    public class DisplayData 
    {
        #region 私有属性
        private skillnew_data m_Config;
        private float m_TotalTime;
        private Vector3Int m_CacheVec3Int = Vector3Int.zero;
        private Vector3 m_CacheVec3 = Vector3.zero;


        private LinkedList<FrameEvent> m_EventList;
        private Queue<LinkedListNode<FrameEvent>> m_Cache;
        #endregion

        #region 共有属性
        public skillnew_data Config { get { return m_Config; } }

        public float TotalTime { get { return m_TotalTime; } }

        public bool Valid { get { return m_Config != null; } }
        #endregion

        public DisplayData()
        {
            m_EventList = new LinkedList<FrameEvent>();
            m_Cache = new Queue<LinkedListNode<FrameEvent>>();
        }

        public void Release()
        {
            m_EventList.Clear();
            m_Cache.Clear();
        }


        public void SetSkill(int skill_id)
        {
            m_Config = SkillController.SkillCfg.Get<skillnew_data>(skill_id);
            if (m_Config == null) return;

            for(int i=0;i<m_Config.SkillActionList.Count;i++)
            {
                SkillActionInfo info = m_Config.SkillActionList[i];
                m_TotalTime += info.ActionTime;
            }

            m_TotalTime = m_TotalTime * 0.001f;
            ResetEventList();
        }

        /// <summary>
        /// 根据时间获取动画名字
        /// </summary>
        /// <param name="targetTime">时间，单位S</param>
        /// <param name="timeOff"></param>
        /// <returns></returns>
        public string GetAnimNameByTime(float targetTime,out float timeOff)
        {
            timeOff = 0;
            if (m_Config == null) return string.Empty;
            targetTime = targetTime * 1000;
            float time = 0;

            for(int i=0;m_Config.SkillActionList != null && i<m_Config.SkillActionList.Count;i++)
            {
                timeOff = time;
                SkillActionInfo info = m_Config.SkillActionList[i];
                time += info.ActionTime;
                if (time > targetTime)
                    return GameEditor.EditorConfigUtil.GetAnimNameByPath(info.ActionPath);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取位置偏移
        /// </summary>
        /// <param name="time">目标时间，单位S</param>
        /// <returns></returns>
        public Vector3 GetPositionOffset(float time)
        {
            time = time * 1000f;
            if (!m_Config.HaveRootmotion) return Vector3.zero;

            float offsetTime = 0;
            m_CacheVec3Int = Vector3Int.zero;
            for(int i=0;i<m_Config.SkillActionList.Count;i++)
            {
                SkillActionInfo info = m_Config.SkillActionList[i];
                float deltaTime = time - offsetTime;
                for(int j=0;j<info.MotionInfos.Count;j++)
                {
                    MotionInfo motion = info.MotionInfos[j];
                    //前面的动作不要
                    if(motion.MotionTime <= info.ActionStartPoint)
                    {
                        continue;
                    }

                    if(motion.MotionTime >= deltaTime)
                    {
                        m_CacheVec3.Set((m_CacheVec3Int.x)* 0.001f,(m_CacheVec3Int.y)* 0.001f,(m_CacheVec3Int.z)* 0.001f);
                        return m_CacheVec3;
                    }
                    m_CacheVec3Int.Set(m_CacheVec3Int.x+ motion.PosX,m_CacheVec3Int.y+ motion.PosY,m_CacheVec3Int.z+motion.PosZ);
                }
                offsetTime += info.ActionTime;
            }
            m_CacheVec3.Set((m_CacheVec3Int.x)* 0.001f,(m_CacheVec3Int.y)* 0.001f,(m_CacheVec3Int.z)* 0.001f);
            return m_CacheVec3;
        }

        public string GetFirstAnimName()
        {
            if (m_Config == null || m_Config.SkillActionList.Count <= 0) return string.Empty;
            return GameEditor.EditorConfigUtil.GetAnimNameByPath(m_Config.SkillActionList[0].ActionPath);
        }
        /// <summary>
        /// 重置事件列表
        /// </summary>
        /// <param name="timePast"></param>
        public void ResetEventList(float timePast=0)
        {

        }
        /// <summary>
        /// 检查事件触发
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timeStep"></param>
        /// <param name="timePast"></param>
        public bool CheckEvent(List<FrameEvent> evt,float timeStep,float timePast)
        {
            if (m_Config == null) return false;
            timeStep = ConfigUtil.ConvertTimeBack(timeStep);
            timePast = ConfigUtil.ConvertTimeBack(timePast);

            float prevTime = timePast - timeStep;
            foreach(var frameEvent in m_Config.Evts)
            {
                if(CheckEvent(frameEvent,prevTime,timePast))
                {
                    evt.Add(frameEvent);
                }
            }

            foreach(var action in m_Config.SkillActionList)
            {
                foreach(var frameEvent in action.FrameEvents)
                {
                    if(CheckEvent(frameEvent,prevTime,timePast))
                    {
                        evt.Add(frameEvent);
                    }    
                }
                timePast -= action.ActionTime;
                prevTime -= action.ActionTime;
            }

            return evt.Count > 0;
        }


        private bool CheckEvent(FrameEvent evt,float prevTime,float timePast)
        {
            //起点出发
            if(evt.EventTimePoint ==0 && prevTime ==0)
            {
                return true;
            }

            return evt.EventTimePoint > prevTime && evt.EventTimePoint <= timePast;
        }

    }


}