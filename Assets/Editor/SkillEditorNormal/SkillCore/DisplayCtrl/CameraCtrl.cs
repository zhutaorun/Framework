using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    //相机控制器
    public class CameraCtrl : CtrlItem
    {
        private const int DELAY_FRAME = 5;

        private Camera m_Camera;
        private Transform m_CameraTransform;
        private Vector3 m_OrigePosition;
        private int m_Delay = 0;

        public Camera MainCamera { get { return m_Camera; } }

        public override bool IsEnd { get { return false; } }

        public CameraCtrl(SkillDisplayManager manager) :base(manager)
        {

        }

        public void SetCamera(Camera cam)
        {
            m_Camera = cam;
            m_CameraTransform = m_Camera.transform;
            m_OrigePosition = m_CameraTransform.position;
        }

        public override void Update(float deltaTime)
        {
            if (!m_CameraTransform) return;
            //if(Shake.ShakeManager.Instance.CameraShaking)
            //{
            //    m_CameraTransform.position = m_OrigePosition + Shake.ShakeManager.Instance.GetCameraOffset(m_CameraTransform,deltaTime);
            //}
            //else
            {
                m_CameraTransform.position = m_OrigePosition;
            }
            if(Application.isPlaying)
            {
                if(m_Camera.targetTexture && !m_Camera.enabled && Playing)
                {
                    m_Delay++;
                    if(m_Delay> DELAY_FRAME)
                    {
                        m_Camera.enabled = true;
                        m_Delay = 0;
                    }

                    if((!m_Camera.targetTexture || !Playing) && m_Camera.enabled)
                    {
                        m_Delay++;
                        if(m_Delay > DELAY_FRAME)
                        {
                            m_Camera.enabled = false;
                            m_Delay = 0;
                        }
                    }
                }
            }
        }

        public override void Release()
        {
            if(m_Camera)
            {
                m_Camera.targetTexture = null;
                m_Camera = null;
            }
            m_CameraTransform = null;
        }
    }

}