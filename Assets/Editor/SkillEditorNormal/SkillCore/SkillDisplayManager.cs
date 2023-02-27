using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    public class MonoBindInfo
    {
        #region 控件绑定变量声明，自动生成请勿手改
        public GameObject Camera;
        public Transform ModelParent;
        public Transform EffectParent;
        public GameObject[] Ground;
        #endregion
    }

    [ExecuteInEditMode]
    public class SkillDisplayManager : SingletonNew<SkillDisplayManager>,IUpdate
    {
        #region 私有信息
        private MonoBindInfo m_BindInfo;//节点绑定信息
        private DisplayData m_SkillData;//技能数据

        private Dictionary<Type, CtrlItem> m_CtrlDict;//所有控制组件
        private GameObject m_Root;//根节点
        private bool m_IsPlaying;//技能展示中

        private Action m_EndCallBack;//播放结束回调
        private float m_CurrentPlayTime;//当前播放时长 单位S
        #endregion

        #region 公关获取
        public DisplayData SkillData { get { return m_SkillData; } }

        public float TimeSpeed { get; set; }//播放速度

        public bool IsPlaying { get { return m_IsPlaying; } }

        public float CurrentTime { get { return m_CurrentPlayTime; } }

        public T GetCtrl<T>() where T:class
        {
            return m_CtrlDict[typeof(T)] as T;
        }

        public MonoBindInfo BindInfo { get { return m_BindInfo; } }

        #endregion
        public override void Init()
        {
            base.Init();
            Debug.Log("SkillDisplayManager Init");
            TimeSpeed = 1;
            m_BindInfo = new MonoBindInfo();
            m_SkillData = new DisplayData();
            m_CtrlDict = new Dictionary<Type, CtrlItem>();

            m_CtrlDict.Add(typeof(ModelCtrl), new ModelCtrl(this));
            m_CtrlDict.Add(typeof(EffectCtrl), new EffectCtrl(this));
            m_CtrlDict.Add(typeof(CameraCtrl), new CameraCtrl(this));
            m_CtrlDict.Add(typeof(BulletCtrl), new BulletCtrl(this));
            m_CtrlDict.Add(typeof(TrapCtrl),new TrapCtrl(this));
            m_CtrlDict.Add(typeof(EventCtrl),new EventCtrl(m_SkillData,this));
            m_CtrlDict.Add(typeof(PunchingBagCtrl),new PunchingBagCtrl(this));
        }


        /// <summary>
        /// 消除特效模型等等，不需要显示时调用
        /// </summary>
        private void Clear()
        {
            foreach(var item in m_CtrlDict)
            {
                item.Value.Release();
            }

            if(m_Root)
            {
                Hooks.ResourceManager.Recycle(m_Root);
                m_Root = null;
            }

            m_IsPlaying = false;
        }

        public void Play()
        {
            m_IsPlaying = true;
            if(m_CurrentPlayTime>=m_SkillData.TotalTime)
            {
                m_CurrentPlayTime = 0;
            }
        }

        public void Play(int skill_id,Action endcall)
        {
            m_EndCallBack?.Invoke();
            m_EndCallBack = null;
            m_IsPlaying = true;
            m_EndCallBack = endcall;
            m_SkillData.SetSkill(skill_id);
            m_CurrentPlayTime = 0;
        }

        public void Pause()
        {
            m_IsPlaying = !m_IsPlaying;
        }

        public void SetSkill(int skill_id)
        {
            m_IsPlaying = false;
            m_EndCallBack?.Invoke();
            m_EndCallBack = null;
            m_SkillData.SetSkill(skill_id);
            SetToBegin();
        }

        public void SetToBegin()
        {
            m_CurrentPlayTime = 0;
            string animName = m_SkillData.GetFirstAnimName();
            GetCtrl<ModelCtrl>().SampleAnimFrame(animName,0);
            GetCtrl<ModelCtrl>().SetPosition(Vector3.zero);
        }

        public void SetToTime(float time)
        {
            m_CurrentPlayTime = time;
            if (m_CurrentPlayTime < 0)
                m_CurrentPlayTime = 0;
            if (m_CurrentPlayTime >= m_SkillData.TotalTime)
                m_CurrentPlayTime = 0;

            float timeOff = 0;
            string animName = m_SkillData.GetAnimNameByTime(m_CurrentPlayTime,out timeOff);
            timeOff = timeOff * 0.001f;
            if(animName == string.Empty)
            {
                return;
            }
            GetCtrl<ModelCtrl>().SampleAnimFrame(animName,m_CurrentPlayTime-timeOff);
            Vector3 position = m_SkillData.GetPositionOffset(m_CurrentPlayTime);
            GetCtrl<ModelCtrl>().SetPosition(position);
        }


        public void Stop()
        {
            OnSkillEnd();
        }

        private void OnSkillEnd()
        {
            m_IsPlaying = false;
            SetToBegin();
            m_EndCallBack?.Invoke();
            m_EndCallBack = null;
        }

        public void Update()
        {
            //非运行时不自动update
            if (Application.isPlaying)
                TryUpdateSkill(Time.deltaTime);
        }

        public void TryUpdateSkill(float deltaTime)
        {
            if(!m_SkillData.Valid)
            {
                return;
            }

            if(!m_IsPlaying)
            {
                if(!(m_CurrentPlayTime< m_SkillData.TotalTime && m_CurrentPlayTime>0))
                {
                    foreach(var item in m_CtrlDict)
                    {
                        if (!item.Value.IsEnd)
                            item.Value.Update(deltaTime);
                    }
                }
                return;
            }

            UpdateSkill(deltaTime);
        }

        public void UpdateSkill(float deltaTime)
        {
            if (!SkillData.Valid) return;

            m_CurrentPlayTime += deltaTime;
            foreach(var item in m_CtrlDict)
            {
                item.Value.Update(deltaTime);
            }

            GetCtrl<EventCtrl>().TryTriggerEvent(m_CurrentPlayTime,deltaTime);

            float timeOff = 0;
            string animName = m_SkillData.GetAnimNameByTime(m_CurrentPlayTime, out timeOff);
            timeOff = timeOff * 0.001f;
            if (animName == string.Empty)
            {
                OnSkillEnd();
                return;
            }
            GetCtrl<ModelCtrl>().SampleAnimFrame(animName, m_CurrentPlayTime - timeOff);
            Vector3 position = m_SkillData.GetPositionOffset(m_CurrentPlayTime);
            GetCtrl<ModelCtrl>().SetPosition(position);
            if(m_CurrentPlayTime >= m_SkillData.TotalTime)
            {
                OnSkillEnd();
            }
        }
    }
}
