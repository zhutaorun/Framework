using GameFrame.Skill.Component;
using GameFrame.Skill.Utility;
using SkillnewConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameFrame.Skill.GameSystem
{
    //处理Entity特效管理
    public class SystemEffectManager :ISystem
    {
        public void Update(float delTime)
        {
            Component<ComponentEffectManager> components = SkillController.Component.GetComponents<ComponentEffectManager>();
            if(components != null)
            {
                ComponentEffectManager component;
                for(int i=0,imax = components.DataCount;i<imax;i++)
                {
                    component = components.GetComponent(i);
                    if (component == null)
                        continue;
                    UpdateControlEffects(i,component,delTime);
                }
            }
            UpdateControlEffects(-1, SkillController.Component.WorldEffectComponent, delTime);
        }

        private void UpdateControlEffects(int entity,ComponentEffectManager effectManagerComponent,float delTime)
        {
            for(int i= effectManagerComponent.ControllEffects.Count-1;i>=0;i--)
            {
                ComponentEffect controllEffect = effectManagerComponent.ControllEffects[i];
                //检查无效的删除
                if(!controllEffect.Valid)
                {
                    effectManagerComponent.RemoveControlEffectAt(i);
                    continue;
                }
                //检查没加载资源的加载资源
                if(!controllEffect.Inited)
                {
                    if(controllEffect.EffectCfg == null || controllEffect.LoadOver)
                    {
                        return;
                    }
                    string path = UtilityEffect.GetEffectAssetPath_Runtime(controllEffect.EffectCfg.Path);
                    LoadEffect(entity,path,effectManagerComponent,controllEffect.EffectUID);
                    controllEffect.Inited = true;
                }
                if (controllEffect.EffectObj == null)
                    continue;

                //更新特效生命周期
                if(controllEffect.EffectElapse == 0)
                {
                    if (controllEffect.AttachEntity >= 0)
                        OnEntityEffectStart(controllEffect.AttachEntity, controllEffect);
                    else
                        OnMapEffectStart(entity,controllEffect);

                    //先设置位置在激,防止拖尾乱跑
                    if(controllEffect.EffectObj)
                    {
                        controllEffect.EffectObj.SetActive(true);
                    }
                }
                controllEffect.EffectElapse += delTime;

                //暂时都按时间结束
                if (effectManagerComponent.ControllEffects[i].EffectCfg.EffectOverTypes.Contains(EOverType.OnTimeOver) || effectManagerComponent.ControllEffects[i].EffectCfg.EffectOverTypes.Count==0)
                {
                    if(controllEffect.EffectElapse>= controllEffect.EffectCfg.LifeTime)
                    {
                        effectManagerComponent.RemoveControlEffectAt(i);
                    }
                }
            }//for
        }

        private void OnEntityEffectStart(int entity,ComponentEffect controllEffect)
        {
            ComponentTransform transComponent = SkillController.Component.GetComponent<ComponentTransform>(entity);
            //播放
            if(controllEffect.EffectCfg.OnEntity)
            {
                Transform affach = transComponent.GetCacheDummy(controllEffect.EffectCfg.PosName);
                if (affach)
                    affach = transComponent.RootGameObjTransform;

                //Vector3 scale = controllEffect.EffectObjTrans.localScale;
                controllEffect.EffectObjTrans.parent = affach;
                //controllEffect.EffectObjTrans.localScale = scale;

                controllEffect.EffectObjTrans.localRotation = Quaternion.identity;
                controllEffect.EffectObjTrans.localPosition = controllEffect.EffectObjTrans.localRotation * controllEffect.EffectOffset;
                controllEffect.StartLocalPos = controllEffect.EffectObjTrans.localPosition;

            }
            else
            {
                controllEffect.EffectObjTrans.parent = transComponent.RootGameObjTransform.parent;
                Quaternion quaternion = transComponent.RootGameObjTransform.rotation;
                controllEffect.EffectObjTrans.localRotation = quaternion;
                controllEffect.EffectObjTrans.localPosition = transComponent.RootGameObjTransform.position + (quaternion* controllEffect.EffectOffset);
                controllEffect.StartLocalPos = controllEffect.EffectObjTrans.localPosition;
            }

            //根据角色缩放
            if(controllEffect.EffectCfg.ScaleWithUnit)
            {
                ComponentEffectManager effectManager = SkillController.Component.GetComponent<ComponentEffectManager>(controllEffect.AttachEntity);
                if(effectManager !=null)
                {
                    controllEffect.EffectObjTrans.localScale = transComponent.RootGameObjTransform.localScale;
                }
            }
        }

        private void OnMapEffectStart(int entity,ComponentEffect controllEffct)
        {
            controllEffct.EffectObjTrans.localRotation = controllEffct.DirOffset;
            controllEffct.EffectObjTrans.localPosition = controllEffct.EffectOffset;
        }

        private void LoadEffect(int entity,string path,ComponentEffectManager effectManagerComponent,uint effectUID)
        {
            Action<GameObject> callback = delegate (GameObject obj)
            {
                ComponentEffect curEffect = effectManagerComponent.GetControlEffectByUID(effectUID);
                if (curEffect != null && curEffect.Valid && curEffect.EffectObj == null)
                {
                    curEffect.LoadOver = true;
                    if (obj == null)
                        return;
                    curEffect.EffectObj = obj;
                    curEffect.EffectObjTrans = obj.transform;
                    curEffect.EffectObjTrans.localScale = Vector3.one * (curEffect.EffectCfg.Scale == 0 ? 1 : curEffect.EffectCfg.Scale) * ComponentEffect.EffectScale;

                    //特效的layer强制设为TransparentFX,避免受场景后处理影响
                    SetLayer(obj, LayerMask.NameToLayer("TransparentFx"));
                    //取消实时阴影
                    Renderer[] rs = obj.GetComponentsInChildren<Renderer>();
                    for (int ri = 0; ri < rs.Length; ri++)
                    {
                        rs[ri].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
                else if (obj != null)
                {
                    //已经不需了
                    Hooks.ResourceManager.Recycle(obj);
                }
            };
            Hooks.ResourceManager.InstantiateAsync(path,callback,false);
        }

        public static void SetLayer(GameObject go,int layer)
        {
            go.layer = layer;
            Transform t = go.transform;

            for(int i=0,imax = t.childCount;i<imax;++i)
            {
                Transform child = t.GetChild(i);
                SetLayer(child.gameObject,layer);
            }
        }
    }
}
