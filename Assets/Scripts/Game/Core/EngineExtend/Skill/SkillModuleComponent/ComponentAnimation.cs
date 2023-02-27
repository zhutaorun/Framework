using GameFrame.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    public class ComponentAnimation : PoolClass
    {
        public Animation CurAnimation;
        public string CurAnimName;
        public AnimationState CurAniState;//当前在播动作
        public List<string> CurLoadingAnim = new List<string>();//当前正在加载的动画名

        public string CommonAnimationPath { get; private set; }

        public string SkillAnimationPath { get; private set; }

        public float CurAnimFadeTime { get; private set; }

        public bool CurAnimIsSkill { get; private set; }

        public float CurAnimStartTime { get; private set; }

        public bool Inited;//是否初始化
        public bool Dirty;//是否有更新

        private bool _animPaused = false;//动画是否暂停
        private float _animLastSpeed = 1.0f;//上次动画速度
        public float _animSpeed = 1.0f;//当前动画速度
        public bool AnimSpeedDirty = false;//动画速度又更新
        public string LastAnimName = string.Empty;

        public string CurModelSpineAnimName = string.Empty;//当前上半身动画
        public string CurModelPelviesAnimName = string.Empty;//当前下半身动画
        public bool MixedAnimation = false;

        public float CurAnimSpeed { get { return _animSpeed; } }

        //保证状态切换成功再调相应的动作
        public void SetStateAnimation(string aniName, float fadeTime, float actionStartPoint, bool isSkill, bool force, float speed = 1.0f)
        {
            if (aniName == string.Empty)
            {
                return;
            }
            if (!string.Equals(aniName, CurAnimName) || force)
            {
                CurAnimName = aniName;
                CurAnimFadeTime = fadeTime;
                CurAnimIsSkill = isSkill;
                CurAnimStartTime = actionStartPoint;
                SetAnimSpeed(speed);
                Dirty = true;
            }
        }

        public void SetCommonAnimationPath(string path)
        {
            CommonAnimationPath = path;
        }

        public void SetSkillAnimationPath(string path)
        {
            SkillAnimationPath = path;
        }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetAnimSpeed(float speed)
        {
            if (_animPaused)
            {
                //不设置当前，设置上次
                _animLastSpeed = speed;
            }
            else
            {
                _animSpeed = speed;
                AnimSpeedDirty = true;
            }
        }

        /// <summary>
        /// 动画暂停
        /// </summary>
        public void SetPauseAnim()
        {
            if (_animPaused) return;
            _animPaused = true;

            _animLastSpeed = _animSpeed;
            _animSpeed = 0;
            AnimSpeedDirty = true;
        }


        /// <summary>
        /// 动画恢复
        /// </summary>
        public void SetResumAnim()
        {
            if (_animPaused)
            {
                _animPaused = false;
                _animSpeed = _animLastSpeed;
                AnimSpeedDirty = true;
            }
        }


        public void InitAnimation(GameObject target)
        {
            if (CurAnimation == null)
            {
                CurAnimation = target.GetComponentInChildren<Animation>();
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
            CurLoadingAnim.Clear();
            CurAnimation = null;
            CurAnimName = string.Empty;
            Inited = false;
            Dirty = false;
            _animPaused = false;
            _animSpeed = 1.0f;
            _animLastSpeed = 1.0f;
            CurAniState = null;
            LastAnimName = string.Empty;
            CurAnimStartTime = 0;
            MixedAnimation = false;
            CurModelSpineAnimName = string.Empty;
            CurModelPelviesAnimName = string.Empty;
        }
    }

}