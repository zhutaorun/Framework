using GameFrame.Skill.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.GameSystem
{
    public class SystemAnimation : ISystem
    {
        private NoGcString _tempString = new NoGcString(200);

        public void Update(float delTime)
        {
            Component<ComponentAnimation> components = SkillController.Component.GetComponents<ComponentAnimation>();
            if(components!=null)
            {
                ComponentAnimation component;
                for(int i=0,imax = components.DataCount;i<imax;i++)
                {
                    component = components.GetComponent(i);
                    if (component == null)
                        continue;
                    UpdateAnimation(i,components.GetComponent(i),delTime);
                }
            }
        }

        public void UpdateAnimation(int key,ComponentAnimation animComponent,float delaTime)
        {
            if (animComponent.CurAnimation == null)
                return;
            //if(!animComponent.Inited)
            //{
            //    if(animComponent.CurAnimation == null)
            //    {
            //        animComponent.CurAnimation = modelComponent.CurModelObj.GetComponent<Animation>();
            //        if (animComponent.CurAnimation == null)
            //            return;
            //        InitAnimationPath(animComponent,key);
            //        animComponent.Inited = true;
            //    }
            //}

            //暂时技能动作配置全路径
            UpdateCurAnimation(key,delaTime,animComponent);
            if(!animComponent.Dirty)
            {
                return;
            }
            if (DoPlayAnimation(key, animComponent)) animComponent.Dirty = false;
        }


        /// <summary>
        /// 初始化动作资源路径
        /// </summary>
        /// <param name="animComponent"></param>
        /// <param name="entity"></param>
        private void InitAnimationPath(ComponentAnimation animComponent,int entity)
        {
            //animComponent.SetCommonAnimationPath(string.Copy(GetCommonAnimationPath(configComponent)));
            //animComponent.SetSkillAnimationPath(string.Copy(GetSkillAnimationPath(configComponent)));
            //if (isPlayer)
            //{
            //    string[] aniNames = GameConst.RunArray;
            //    for (int index = 0, length = aniNames.Length; index < length; index++)
            //    {
            //        AnimationState animState = animComponent.CurAnimation[animComponent.CurAnimName];
            //        if (animState == null)
            //        {
            //            基本的移动直接给角色
            //            if (!animComponent.CurLoadingAnim.Contains(aniNames[index]))
            //            {
            //                animComponent.CurLoadingAnim.Add(aniNames[index]);
            //                AddAnimationClip(entity, animComponent.CurAnimation, aniNames[index], animComponent.CommonAnimationPath);
            //            }
            //        }
            //    }
            //}
        }


        public void UpdateCurAnimation(int entity,float delaTime,ComponentAnimation animComponent)
        {
            //更新速度
            if(animComponent.AnimSpeedDirty)
            {
                animComponent.AnimSpeedDirty = false;
                AnimationState animState = animComponent.CurAniState;
                if (animState != null)
                    animState.speed = animComponent.CurAnimSpeed;
            }
        }

        private bool DoPlayAnimation(int entity,ComponentAnimation animComponent)
        {
            if(animComponent.CurAnimation == null || string.IsNullOrEmpty(animComponent.CurAnimName))
            {
                return false;
            }
            AnimationState animState = animComponent.CurAnimation[animComponent.CurAnimName];
            string aniName = animComponent.CurAnimName;
            float fadeTime = animComponent.CurAnimFadeTime;
            if(animState == null)
            {
                if(!animComponent.CurLoadingAnim.Contains(aniName))
                {
                    animComponent.CurLoadingAnim.Add(aniName);
                    string pathBase = animComponent.CurAnimIsSkill ? animComponent.SkillAnimationPath : animComponent.CommonAnimationPath;
                    if (pathBase == null) pathBase = string.Empty;
                    AddAnimationClip(entity,animComponent.CurAnimation,aniName,pathBase);
                }
                return false;
            }
            Play(entity,animComponent);
            return true;
        }

        private void Play(int entity,ComponentAnimation animComponent)
        {
            animComponent.CurAniState = animComponent.CurAnimation[animComponent.CurAnimName];
            animComponent.AnimSpeedDirty = true;
            animComponent.CurAnimation.Stop(animComponent.CurAnimName);
            float fadeTime = animComponent.CurAnimFadeTime;
            //if ((animComponent.LastAnimName.Contains("run") && animComponent.CurAnimName.Contains("stand")))
            //{
            //    fadeTime = 0.5f;//暂时特殊
            //}
            //else if ((animComponent.LastAnimName.Contains("stand") && animComponent.CurAnimName.Contains("run")))
            //    fadeTime = 0.02f;//暂时特殊

            float length = animComponent.CurAniState.length;
            float normalizedTime = 0;
            if(length > 0 && animComponent.CurAnimStartTime>0)
            {
                normalizedTime = animComponent.CurAnimStartTime / length;
                if (normalizedTime > 1) normalizedTime = 1;
                animComponent.CurAniState.normalizedTime = normalizedTime;
            }
            animComponent.CurAnimation.CrossFade(animComponent.CurAnimName,fadeTime,PlayMode.StopSameLayer);

            //暂时不分上下半身
            //ResetMixedAnimation(entity,animComponent);
            //ComponentSkill skill = _room.ComponentManagerObj.SkillComponents.GetComponent(entity);
            //if(animComponent.CurAnimIsSkill && skill.CheckMoveAction && skill.CheckPlayRunAction)
            //{
            //    if(skill.StopMove)
            //    {
            //        animComponent.CurAnimation[animComponent.CurAnimName].layer = 0;
            //        animComponent.CurAnimation[animComponent.CurAnimSpeed].time = animComponent.CurAnimation[animComponent.LastAnimName].time;
            //        animComponent.CurAnimation.CrossFade(animComponent.CurAnimation,0.2f,PlayMode.StopAll);
            //        animComponent.CurModelPelviesAnimName = animComponent.CurAnimName;
            //        animComponent.CurModelSpineAnimName = animComponent.CurAnimName;
            //    }
            //    else
            //    {
            //        animComponent.MixedAnimation = true;
            //        //播下半身动画
            //        PlayRunAnimation(entity,animComponent,skill,fadeTime);
            //        //播上半身动画
            //        MixingAnimation(entity,animComponent,fadeTime,skill.SkillStart);
            //    }
            //}
            //else
            //{
            //    animComponent.CurAnimation[animComponent.CurAnimation].layer = 0;
            //    animComponent.CurAnimation.CrossFade(animComponent.CurAnimName,fadeTime,PlayMode.StopAll);
            //    animComponent.CurModelPelviesAnimName = animComponent.CurAnimName;
            //    animComponent.CurModelSpineAnimName = animComponent.CurAnimName;
            //}
            animComponent.LastAnimName = animComponent.CurAnimName;
            
        }


        public void AddAnimationClip(int entity,Animation curAnimation,string animName,string pathBase)
        {
            _tempString.Clear();
            _tempString.Append(ResourcePath.AddressableRoot);
            _tempString.Append(pathBase);
            _tempString.Append(animName);
            _tempString.Append(".anim");
            string path = string.Copy(_tempString.getString());
            Hooks.ResourceManager.LoadAssetAsync<AnimationClip>(path,(AnimationClip clip)=> 
            {
                if(curAnimation != null && clip!= null)
                {
                    //如果有
                    Debug.LogError("AddAnimationClip loaded have"+animName);
                }
                else
                {
                    clip.legacy = true;
                    curAnimation.AddClip(clip,animName);
                    ComponentAnimation animComponent = SkillController.Component.GetComponent<ComponentAnimation>(entity);
                    if (animComponent != null)
                        animComponent.CurLoadingAnim.Remove(animName);
                }
            });
        }

        //public void MixingAnimation(int entity,ComponentAnimation animComponent,float fadeTime,bool skillStart)
        //{
        //    ComponentModel model = _room.ComponentManagerObj.ModelComponents.GetComponent(entity);
        //    if(model != null && model.ModelSpine !=null)
        //    {
        //        animComponent.CurAnimation[animComponent.CurAnimation].AddMixingTransform(model.ModelSpine);
        //        animComponent.CurAnimation[animComponent.CurAnimation].layer = 1;
        //        animComponent.CurModelSpineAnimName = animComponent.CurAnimName;
        //        if(!skillStart)
        //        {
        //            animComponent.CurAnimation[animComponent.CurAnimName].time = animComponent.CurAnimation[animComponent.LastAnimName].time;
        //        }
        //        animComponent.CurAnimation.CrossFade(animComponent.CurAnimName,fadeTime,PlayMode.StopSameLayer);
        //    }
        //}

        //public void ResetMixedAnimation(int entity,ComponentAnimation animComponent)
        //{
        //    if(animComponent.MixedAnimation)
        //    {
        //        ComponentModel model = _room.ComponentManagerObj.ModelComponents.GetComponent(entity);
        //        if (model == null)
        //            return;
        //        animComponent.CurAnimation[animComponent.CurModelSpineAnimName].RemoveMixingTransform(model.ModelSpine);
        //        animComponent.CurAnimation[animComponent.CurModelSpineAnimName].layer = 0;
        //        animComponent.CurAnimation[animComponent.CurModelPelviesAnimName].RemoveMixingTransform(model.ModelPelvis);
        //        animComponent.CurAnimation[animComponent.CurModelPelviesAnimName].layer = 0;
        //        animComponent.MixedAnimation = false;
        //    }
        //}

        //public void PlayRunAnimation(int entity,ComponentAnimation animComponent,ComponentSkill skill,float fadeTime)
        //{
        //    if(skill.CurActionAnimIndex<0)
        //    {
        //        return;
        //    }
        //    string animName = GameConst.RunArray[skill.CurActionAnimIndex];
        //    ComponentModel model = _room.ComponentManagerObj.ModelComponents.GetComponent(entity);
        //    AnimationState state = animComponent.CurAnimation[animName];
        //    if (model != null && model.ModelPelvis != null && animName != null && state != null)
        //    {
        //        animComponent.CurModelPelviesAnimName = animName;
        //        animComponent.CurAnimation.CrossFade(animName, fadeTime, PlayMode.StopSameLayer);
        //    }
        //}
    }
}
