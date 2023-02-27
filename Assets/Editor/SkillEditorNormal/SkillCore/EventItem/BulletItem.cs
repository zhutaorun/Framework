using UnityEngine;
using BulletConfig;
using SkillnewConfig;

namespace GameFrame.SkillDisplay
{

    public class BulletItem : BaseItem
    {
        private Vector3[] autoTaregtPosition = {Vector2.up,Vector3.down,Vector3.left,Vector3.right };

        public EffectItem Effect;
        public bullet_data Config;
        private float TotalLife;
        private float LifeTime;
        private Vector3 m_StartPosition;
        private Vector3 m_Dir;
        private float m_Speed;
        private float m_RotateSpeed;
        private GameObject m_Root;
        private Vector3 m_AutoDesPosition;

        public bool IsOver { get; private set; }

        public override void OnUse()
        {
            base.OnUse();
        }
        
        public void Init(bullet_data cfg,Vector3 position,Vector3 dir)
        {
            if(!m_Root)
            {
                m_Root = new GameObject();
            }
            m_Root.name = cfg.Name + cfg.Id;
            Config = cfg;
            TotalLife = (float)cfg.BulletRangeValue / cfg.BulletSpeedValue;
            LifeTime = 0;
            IsOver = false;

            float range = ConfigUtil.ConvertLength(cfg.BulletRangeValue);
            m_RotateSpeed = ConfigUtil.ConvertLength(cfg.DirSpeed);
            m_Speed = ConfigUtil.ConvertLength(cfg.BulletSpeedValue);
            m_StartPosition = position;
            if(cfg.LockTarget)
            {
                dir = (GetCtrl<PunchingBagCtrl>().Model.transform.position - position).normalized;
                dir = new Vector3(dir.x,0,dir.z);
            }
            m_Dir = dir;

            int randIndex = Random.Range(0,autoTaregtPosition.Length);
            float offset = Random.Range(0, range);
            m_AutoDesPosition = position + m_Dir * Mathf.Lerp(0.5f * range, range, offset / range) + autoTaregtPosition[randIndex] * offset;

            m_Root.transform.position = position;
            m_Root.transform.forward = dir;
            Effect = GetCtrl<EffectCtrl>().CreateEffect((int)Config.EffectId,false,Vector3.zero,m_Root.transform);
            if(Effect!= null && Effect.Obj != null)
            {
                Effect.Obj.transform.localRotation = Quaternion.identity;
            }
        }

        public void Update(float deltaTime)
        {
            if(LifeTime <=0)
            {
                TriggerEvent(EOutputTime.OnStart);
            }

            LifeTime += deltaTime;
            AutoRotate();
            Vector3 stepdelta = m_Speed * m_Dir * deltaTime;
            m_Root.transform.position += stepdelta;
            if(Config.AutoTarget && m_RotateSpeed >0)
            {
                float step = stepdelta.magnitude;
                Vector3 delta = m_Root.transform.position - m_AutoDesPosition;
                if(delta.magnitude< step)
                {
                    TriggerEvent(EOutputTime.OnEnd);
                    IsOver = true;
                }
            }
            if(Config.LockTarget)
            {
                Vector3 pos = GetCtrl<PunchingBagCtrl>().Model.transform.position;
                if(Vector2.Distance(new Vector2(m_Root.transform.position.x,m_Root.transform.position.z),new Vector2(pos.x,pos.y))<0.5f)
                {
                    TriggerEvent(EOutputTime.OnEnd);
                    IsOver = true;
                }
            }
            else if(LifeTime >= TotalLife)
            {
                TriggerEvent(EOutputTime.OnEnd);
                IsOver = true;
            }
#if UNITY_EDITOR
            else if(!Application.isPlaying && LifeTime >0)
            {
                Vector3 half = m_Dir * 0.5f;
                SkillRangeShow.Instance.DrawCircle(m_Root.transform.position,ConfigUtil.ConvertLength(Config.BulletRediusValue),1);
                //todo这句话是什么作用
                //SkillRangeShow.Instance.DrawRect(m_Root.transform.position - half,m_Root.transform.position+ half, ConfigUtil.ConvertLength(Config.BulletRediusValue), Quaternion.identity,1);
            }
#endif
            if(Effect != null)
            {
                Effect.SimpleEffect(deltaTime);
                Effect.LifeTime += deltaTime;
            }
        }

        private void AutoRotate()
        {
            if (Config == null)
                return;
            if (!Config.AutoTarget)
                return;

            if (Config.DirSpeed <= 0)
                return;

            Vector3 curpos = m_Root.transform.position;
            Vector3 destDir = (m_AutoDesPosition - curpos).normalized;
            float cos = Vector3.Dot(destDir,m_Dir);
            float deg = Mathf.Acos(cos) * Mathf.Rad2Deg;

            if (deg > m_RotateSpeed)
            {
                m_Dir = Vector3.Lerp(m_Dir, destDir, m_RotateSpeed / deg);
                m_Dir.Normalize();
            }
            else
                m_Dir = destDir;
            m_Root.transform.LookAt(curpos +m_Dir);
        }

        private void TriggerEvent(EOutputTime outputTime)
        {
            foreach(var item in Config.BulletOutputs)
            {
                if(item.OutputTime == outputTime)
                {
                    GetCtrl<EventCtrl>().TriggerOutput(item,m_Root.transform);
                }
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
            if(Effect != null)
            {
                Effect.Release();
                Effect = null;
            }

            if(m_Root)
            {
                GameObject.DestroyImmediate(m_Root);
                m_Root = null;
            }
            Config = null;
            IsOver = true;
        }
    }
}
