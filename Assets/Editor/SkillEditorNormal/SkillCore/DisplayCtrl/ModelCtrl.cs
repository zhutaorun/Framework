using System;
using UnitConfig;
using UnityEngine;

namespace GameFrame.SkillDisplay
{
    /// <summary>
    /// 模型加载、控制、播放，动画加载、播放
    /// </summary>
    public class ModelCtrl : CtrlItem
    {
        private GameObject m_Model;
        private int m_UnitID;
        private string m_SkillAnimPath;
        private string m_CommonAnimPath;
        private Animation m_Animation;
        private Transform m_ParentTransform;


        public Transform ModelTransform { get { return m_Model ? m_Model.transform : null; } }

        public GameObject Model { get { return m_Model; } }

        public Animation Anim { get { return m_Animation; } }

        public Vector3 Forward { get { return m_Model == null ? Vector3.forward : Model.transform.forward; } }

        public Vector3 Position { get { return m_Model == null ? Vector3.zero : m_Model.transform.position; } }
        

        public string SkillAnimPath { get { return m_SkillAnimPath; } }


        /// <summary>
        /// 构造.初始化
        /// </summary>
        /// <param name="manager"></param>
        public ModelCtrl(SkillDisplayManager manager):base(manager)
        {

        }

        public override void Update(float deltaTime)
        {

        }

        public void SetParentTransform(Transform parent)
        {
            m_ParentTransform = parent;
        }

        /// <summary>
        /// 准备模型用于显示
        /// </summary>
        /// <param name="unitid"></param>
        public void PrepareModel(uint unitid)
        {
            if(m_UnitID == (int)unitid && m_Model)
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
                if(objModel == null)
                {
                    return;
                }
                OnModelLoad(objModel, unit.UnitType, m_ParentTransform);
            };

            string addressablePath = ResourcePath.AddressableRoot + unit.ModelPath;
            Hooks.ResourceManager.InstantiateAsync(addressablePath, (model)=> 
            {
                m_Model = model;
                callback(m_Model);
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
            //todo 模型加载后处理
            m_Model = objModel;
            m_Model.transform.SetParent(parent);
            m_Model.transform.localPosition = Vector3.zero;
            m_Model.transform.localEulerAngles = new Vector3(0, 180, 0);
            m_Animation = m_Model.GetComponent<Animation>();
        }

        private void TrySetAnimationAsync(string name,Action callback,bool isSkill = true)
        {
            if (!m_Animation || string.IsNullOrEmpty(name))
                return;
            if(m_Animation[name] == null)
            {
                LoadAnimationAsync(name, (clip)=> 
                {
                    if (clip)
                        m_Animation.AddClip(clip, name);
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

            TrySetAnimationAsync(name, () => 
            {
                AnimationClip clip = m_Animation.GetClip(name);
                if (clip == null || m_Model == null) return;
                clip.SampleAnimation(m_Model,time);
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

            Debug.Log("model ctrl release");
        }
    }

}