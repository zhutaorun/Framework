using UnityEngine;
using System;
using UnitConfig;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 模型加载、控制、播放，动画加载、播放
    /// </summary>
    public class PunchingBagCtrl:CtrlItem
    {
        /// <summary>
        /// 角色模型
        /// </summary>
        private GameObject m_Model;
        private int m_UnitID;
        private string m_SkillAnimPath;
        private string m_CommonAnimPath;
        private Animation m_Animation;
        private Transform m_ParentTransform;

        public Transform ModelTransform { get { return m_Model ? m_Model.transform : null; } }

        public GameObject Model { get { return m_Model; } }

        public Animation Anim { get { return m_Animation; } }

        public Vector3 Forwad { get { return m_Model == null ? Vector3.forward : m_Model.transform.forward; } }

        public Vector3 Position { get { return m_Model == null ? Vector3.zero : m_Model.transform.position; } }

        string anim_idle = "idle";
        string anim_hit = "hit";
        float curAnimTime = 0;
        bool beHit = false;

        public string SkillAnimPath { get { return m_SkillAnimPath; } }

        public override bool IsEnd { get { return false; } }

        public PunchingBagCtrl(SkillDisplayManager manager):base(manager)
        {

        }

        public override void Update(float deltaTime)
        {
            if(m_Animation != null)
            {
                if(beHit)
                {
                    var animState = m_Animation[anim_idle];
                    if(animState!= null && animState.length< curAnimTime)
                    {
                        curAnimTime = 0;
                        beHit = false;
                    }
                    SampleAnimFrame(anim_hit,curAnimTime);
                    curAnimTime += deltaTime;
                }
                else
                {
                    var animState = m_Animation[anim_idle];
                    if(animState != null && animState.length< curAnimTime)
                    {
                        curAnimTime = 0;
                    }
                    SampleAnimFrame(anim_idle,curAnimTime);
                    curAnimTime += deltaTime;
                }
            }
        }

        public void BeHit() 
        {
            curAnimTime = 0;
            beHit = true;
        }

        public void SetParentTranform(Transform parent)
        {
            m_ParentTransform = parent;
        }

        /// <summary>
        /// 准备模型用于显示
        /// </summary>
        /// <param name="unitid"></param>
        public void PrepareModel(uint unitid)
        {
            if (m_UnitID == (int)unitid && m_Model)
            {
                return;
            }
            if(m_Model)
            {
                Hooks.ResourceManager.Recycle(m_Model);
                m_Model = null;
            }
            m_UnitID = (int)unitid;
            unit_data unit = SkillController.SkillCfg.Get<unit_data>(m_UnitID);
            SetAnimPath(unit);
            Action<GameObject> callback = delegate (GameObject objModel)
            {
                if (objModel == null)
                {
                    return;
                }
                OnModelLoad(objModel, unit.UnitType, m_ParentTransform);
            };

            string addressablePath = ResourcePath.AddressableRoot + unit.ModelPath;
            Hooks.ResourceManager.InstantiateAsync(addressablePath, (model) => 
            {
                m_Model = model;
                callback(model);
            });
        }

        /// <summary>
        /// 模型加载回调
        /// </summary>
        /// <param name="objModel"></param>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        private void OnModelLoad(GameObject objModel,UnitType type,Transform parent)
        {
            //todo模型加载后处理
            m_Model = objModel;
            m_Model.transform.SetParent(parent);
            m_Model.transform.localPosition = -Vector3.forward * 6;
            m_Model.transform.localEulerAngles = Vector3.zero;
            m_Animation = m_Model.GetComponent<Animation>();
        }

        /// <summary>
        /// 获取动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="isSkill"></param>
        private void TrySetAnimationAsync(string name,Action callback,bool isSkill = true)
        {
            if (!m_Animation || string.IsNullOrEmpty(name))
                return;
            if(m_Animation[name] == null)
            {
                LoadAnimationAsync(name, (clip)=> 
                {
                    if (clip)
                        m_Animation.AddClip(clip,name);
                    callback.Invoke();
                });
            }
            else
            {
                callback.Invoke();
            }
        }


        /// <summary>
        /// 设置动画路径
        /// </summary>
        /// <param name="unit"></param>
        private void SetAnimPath(unit_data unit)
        {
            m_SkillAnimPath = unit.AnimationPath;
        }

        /// <summary>
        /// 异步加载动画资源
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="isSkill"></param>

        private void LoadAnimationAsync(string name,Action<AnimationClip> callback,bool isSkill = true)
        {
            string path = isSkill ? m_SkillAnimPath : m_CommonAnimPath;
            path = ResourcePath.AddressableRoot + path + name + ".anim";
            Hooks.ResourceManager.LoadAssetAsync<AnimationClip>(path, (clip)=> 
            {
                callback.Invoke(clip);
            });
        }

        public void PlayAnim(string name,bool isSkill = true)
        {
            if (!m_Animation) return;
            if (name == string.Empty) return;

            TrySetAnimationAsync(name, () => 
            {
                m_Animation.Play(name);
            },isSkill);
        }

        public void SampleAnimFrame(string name,float time)
        {
            if (!m_Animation) return;
            if(m_Animation.isPlaying)
            {
                m_Animation.Stop();
            }

            TrySetAnimationAsync(name, ()=> 
            {
                AnimationClip clip = m_Animation.GetClip(name);
                if (clip == null) return;
                clip.SampleAnimation(m_Model, time);
            });
        }

        public void SetPosition(Vector3 position)
        {
            if (!m_Model) return;
            m_Model.transform.localPosition = position;
        }

        public override void Release()
        {
            m_UnitID = 0;
            if(m_Model)
            {
                Hooks.ResourceManager.Recycle(m_Model);
                m_Model = null;
            }
            m_Animation = null;

            Debug.Log("PunchingBagCtrl release");
        }
    }
}
