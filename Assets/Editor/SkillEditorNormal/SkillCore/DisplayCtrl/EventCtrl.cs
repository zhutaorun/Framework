using SkillnewConfig;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 事件控制器
    /// </summary>

    public class EventCtrl : CtrlItem
    {
        private DisplayData  m_DisplayData;

        private List<FrameEvent> m_Cache;

        public EventCtrl(DisplayData displayData,SkillDisplayManager manager):base(manager)
        {
            m_DisplayData = displayData;
            m_Cache = new List<FrameEvent>();
        }

        public override void Update(float deltaTime)
        {

        }

        public void TryTriggerEvent(float timePast,float timeStep)
        {
            m_Cache.Clear();
            if(m_DisplayData.CheckEvent(m_Cache,timeStep,timePast))
            {
                ModelCtrl ctrl = GetCtrl<ModelCtrl>();
                if(ctrl == null)
                {
                    Debug.LogError("null modelCtrl,why?");
                }
                foreach(var evt in m_Cache)
                {
                    TriggerEvent(evt, ctrl.ModelTransform);
                }
            }
        }

        private void TriggerEvent(FrameEvent evt,Transform parent)
        {
            switch (evt.EventType)
            {
                case EFrameEventType.CameraShake:
                    CameraShake(evt.FrameEventId);
                    break;
                case EFrameEventType.CameraBlur:
                case EFrameEventType.CameraBlurLow:
                case EFrameEventType.CameraBlurHigh:
                    CameraBlur(evt);
                    break;
                case EFrameEventType.Effect:
                    Effect(evt.Offset, evt.FrameEventId, parent);
                    break;
                case EFrameEventType.SendBullet:
                    SendBullet(evt, parent);
                    break;
                case EFrameEventType.SetTrap:
                    SetTrap(evt, parent);
                    break;
                case EFrameEventType.DirectHurt:
                    if(!Application.isPlaying)
                    {
                        if(evt.DamageData != null && evt.DamageData.Count>0)
                        {
                            eventpick_data evtpick = evt.DamageData[0].EventPick;
                            SkillRangeShow.Instance.ShowRange(evtpick, m_DisplayData.Config, GetCtrl<ModelCtrl>().Model, 5);
                            for(int i=0;i< evt.DamageData.Count;i++)
                            {
                                if(evt.DamageData[i].Outputs != null)
                                {
                                    for(int j =0;j<evt.DamageData[i].Outputs.Count;j++)
                                    {
                                        TriggerOutput(evt.DamageData[i].Outputs[j],parent);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case EFrameEventType.SetBuff:
                    break;
            }
        }

        public void TriggerOutput(OutputItem item,Transform parent)
        {
            TriggerOutput(item.OutputType, item.Value, item.Offset, item.Angle, parent);
        }

        public void TriggerOutput(EOutputType outputType,int id,Global_Int_Vector3 offset,int angle,Transform parent)
        {
            switch(outputType)
            {
                //没有目标,不处理
                case EOutputType.OptBuff:
                    break;
                case EOutputType.OptBullet:
                    SendBullet(id, offset, angle, parent);
                    break;
                case EOutputType.OptEffect:
                    parent = GetCtrl<PunchingBagCtrl>().Model.transform;
                    Effect(offset,id,parent);
                    break;
                case EOutputType.OptTrap:
                    SetTrap(id,offset,angle,parent);
                    break;
                case EOutputType.OptCamShake:
                    CameraShake(id);
                    break;
                case EOutputType.OptDamage:
                    BeHit();
                    break;
            }
        }

        private void BeHit()
        {
            PunchingBagCtrl mctrl = GetCtrl<PunchingBagCtrl>();
            if (mctrl != null)
                mctrl.BeHit();
        }


        private void CameraShake(int evtid)
        {
            //todo 需要新的方案
        }

        private void CameraBlur(FrameEvent evt)
        {

        }

        private void Effect(Global_Int_Vector3 Offset,int id,Transform parent)
        {
            if (Offset == null)
            {
                Offset = new Global_Int_Vector3();
            }
            Vector3 offset = ConfigUtil.ConvertPositionVec(Offset);
            GetCtrl<EffectCtrl>().CreateEffect(id,true,offset,parent);
        }


        private void SendBullet(FrameEvent evt,Transform parent)
        {
            SendBullet(evt.FrameEventId, evt.Offset, evt.Angle, parent);
        }

        private void SendBullet(int id,Global_Int_Vector3 Offset,int angle,Transform parent)
        {
            if(parent == null)
            {
                Debug.LogError("没有角色，释放子弹失败");
                return;
            }
            Vector3 dir = -parent.forward;
            float dig = Mathf.Atan2(dir.z,dir.x) * Mathf.Rad2Deg+ angle;
            dir = new Vector3(Mathf.Cos(dig * Mathf.Deg2Rad),0,Mathf.Sin(dig* Mathf.Deg2Rad));
            Vector3 xdir = Vector3.Cross(new Vector3(0,1,0),(dir));
            Vector3 offset = ConfigUtil.ConvertPositionVec(Offset);

            Vector3 startPosition = parent.position + offset.x * xdir + offset.z * dir + offset.y * Vector3.up;
            GetCtrl<BulletCtrl>().CreateBullet(id,startPosition,dir);
        }

        private void SetTrap(FrameEvent evt,Transform parent)
        {
            SetTrap(evt.FrameEventId, evt.Offset, evt.Angle, parent);
        }

        private void SetTrap(int id,Global_Int_Vector3 Offset,int angle,Transform parent)
        {
            Vector3 dir = -parent.forward;
            float dig = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg + angle;
            dir = new Vector3(Mathf.Cos(dig * Mathf.Deg2Rad), 0, Mathf.Sin(dig * Mathf.Deg2Rad));
            Vector3 xdir = Vector3.Cross(new Vector3(0, 1, 0), (dir));
            Vector3 offset = ConfigUtil.ConvertPositionVec(Offset);

            Vector3 position = parent.position + offset.x * xdir + offset.z * dir + offset.y * Vector3.up;
            GetCtrl<TrapCtrl>().CreateTrap(id, position, dir);
        }

        public override void Release()
        {

        }
    }

}