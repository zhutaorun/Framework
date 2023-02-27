using EffectConfig;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    public class EffectItem : BaseItem
    {
        public effect_data Config;
        public GameObject Obj;
        public bool AutoRelease = false;
        public float LifeTime;
        public Transform ParentTrans;

        public override void OnUse()
        {
            base.OnUse();
        }

        public void Init(effect_data cfg,bool autoRelease,Vector3 offset,Transform parent = null)
        {
            if (cfg == null) return;
            AutoRelease = autoRelease;
            Config = cfg;
            LifeTime = 0;
            ParentTrans = parent;

            Hooks.ResourceManager.InstantiateAsync(GameFrame.Skill.Utility.UtilityEffect.GetEffectAssetPath_Runtime(Config.Path),(obj)=> 
            {
                Obj = obj;
                if(Obj)
                {
                    if(Config.OnEntity)
                    {
                        if(ParentTrans && Config.PosName != string.Empty)
                        {
                            Transform trans = SkillEditorUtils.FindChildByName(Config.PosName,ParentTrans);
                            Obj.transform.SetParent(trans);
                            Obj.transform.localPosition = offset;
                        }
                        else
                        {
                            if(ParentTrans == null)
                            {
                                ParentTrans = GetCtrl<EffectCtrl>().EffectRoot;
                                Obj.transform.position = offset;
                            }
                            else
                            {
                                Obj.transform.SetParent(ParentTrans);
                                Obj.transform.localPosition = offset;
                            }
                        }
                    }
                    else
                    {
                        Obj.transform.SetParent(GetCtrl<EffectCtrl>().EffectRoot);
                        Obj.transform.position = ParentTrans ? ParentTrans.rotation * offset + ParentTrans.position : offset;
                    }
                    Obj.transform.localRotation = Quaternion.identity;
                    if (ParentTrans && Config.ScaleWithUnit)
                        Obj.transform.localScale = Config.Scale * ParentTrans.localScale;
                    else
                        Obj.transform.localScale = Config.Scale * Vector3.one;
                }
            });
        }
    
        public void SetPosition(Vector3 position)
        {
            if (!Obj) return;
            Obj.transform.localPosition = position;
        }

        public void SimpleEffect(float deltaTime)
        {
            if (!Obj) return;
            //运行模式不需要此方法
            if (Application.isPlaying) return;
            bool outReverse = deltaTime < 0;
            float fElapseTime = LifeTime;
            float delta = deltaTime;
            //播放粒子特效
            ParticleSystem[] particles = Obj.GetComponentsInChildren<ParticleSystem>();
            for(int i=0;i< particles.Length;i++)
            {
                ParticleSystem ps = particles[i];
                if(!outReverse && LifeTime>0)
                {
                    ps.Simulate(delta,false,false);
                }
                else
                {
                    ps.Stop();
                    ps.Simulate(LifeTime,false,true);
                }
            }

            //播放动画
            Animator[] animators = Obj.GetComponentsInChildren<Animator>();
            foreach(Animator anim in animators)
            {
                for(int i=0;i< anim.layerCount;i++)
                {
                    AnimatorClipInfo[] aci = anim.GetCurrentAnimatorClipInfo(i);
                    if(aci == null || aci.Length == 0)
                    {
                        continue;
                    }
                    if(!outReverse)
                    {
                        aci[0].clip.SampleAnimation(anim.gameObject,(float)fElapseTime);
                    }
                    else
                    {
                        aci[0].clip.SampleAnimation(anim.gameObject,-1.0f);
                    }
                }
            }

            Animation[] animations = Obj.GetComponentsInChildren<Animation>();
            foreach(var item in animations)
            {
                if(item== null || item.clip == null)
                {
                    Debug.LogError(Obj.name +"'s animation should have an animation clip");
                    continue;
                }

                if(!outReverse)
                {
                    item.clip.SampleAnimation(item.gameObject,(float)fElapseTime);
                }
                else
                {
                    item.clip.SampleAnimation(item.gameObject,-1.0f);
                }
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
            if(Obj)
            {
                Hooks.ResourceManager.Recycle(Obj);
                Obj = null;
            }
            Config = null;
            ParentTrans = null;
            LifeTime = 0;
            AutoRelease = false;
        }
    }
}
