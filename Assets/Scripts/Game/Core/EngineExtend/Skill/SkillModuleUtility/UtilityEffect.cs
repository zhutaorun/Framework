using BuffConfig;
using EffectConfig;
using UnityEngine;
using GameFrame.Skill.Component;
using System.IO;

namespace GameFrame.Skill.Utility
{
    public static class UtilityEffect
    {
        public static uint PlayEffect(int fromEntity, int attachEntity, int effectId, Vector3 offsetpos = default(Vector3), Vector3 dirVec3 = default(Vector3), bool beattackEffect = false)
        {
            if (effectId == 0) return 0;
            if (SkillDefine.DEBUG_OPEN_EFFECT)
            {
                effect_data effectCfg = SkillController.SkillCfg.Get<effect_data>(effectId);
                if (effectCfg == null) return 0;
                bool valid = fromEntity >= 0 && attachEntity >= 0;
                //挂载entity的特效播放
                //需要通过技能控制的特效加入
                if (valid && (effectCfg.OnEntity || !effectCfg.NotRemoveOnSkillOver || effectCfg.RemoveOnSkillActionOver))
                {
                    ComponentEffectManager effectCompnent = SkillController.Component.GetComponent<ComponentEffectManager>(fromEntity);
                    if (effectCompnent != null)
                    {
                        return effectCompnent.AddEffect(effectId, attachEntity, offsetpos, beattackEffect);
                    }
                }
                else
                {
                    Quaternion dir = Quaternion.identity;
                    //不挂载entity的特效播放
                    if (valid)
                    {
                        ComponentTransform transform = SkillController.Component.GetComponent<ComponentTransform>(attachEntity);
                        if (transform.RootGameObjTransform != null)
                        {
                            offsetpos = transform.RootGameObjTransform.rotation * offsetpos;
                            offsetpos += transform.RootGameObjTransform.position;
                            dir = transform.RootGameObjTransform.rotation;
                        }
                    }
                    else
                    {
                        if (dirVec3 != Vector3.zero)
                        {
                            dir = Quaternion.LookRotation(dirVec3);
                        }
                    }
                    return SkillController.Component.WorldEffectComponent.AddEffect(effectId, -1, offsetpos, beattackEffect, dir);
                }
            }
            return 0;
        }

        public static void RemoveEntityEffect(int fromEntity, uint effectUId)
        {
            ComponentEffectManager effectCompnent = SkillController.Component.GetComponent<ComponentEffectManager>(fromEntity);
            if (effectCompnent != null)
            {
                effectCompnent.RemoveEffectByUID(effectUId);
            }
        }

        public static void RemoveWorldEffect(uint effectUId)
        {
            if (SkillController.Component.WorldEffectComponent != null)
            {
                SkillController.Component.WorldEffectComponent.RemoveEffectByUID(effectUId);
            }
        }

        public static void OnBuffUpdateCheckAnimation(int entity, EBuffType buffType, bool logicStart)
        {

        }

        /// <summary>
        /// 获取asset路径
        /// </summary>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public static string GetEffectAssetPath(string subPath)
        {
            if (subPath == null)
                subPath = "";
            return (Path.Combine(SkillDefine.EffectPath, subPath) + ".prefab");
        }

        public static string GetEffectAssetPath_Runtime(string subPath)
        {
            if (subPath == null)
                subPath = "";
            return (Path.Combine(SkillDefine.EffectPath_Runtime, subPath)+".prefab");
        }

        /// <summary>
        /// 获取特效子节点名字,不含后缀
        /// </summary>
        /// <param name="fullPath">Assets 目录(包含Assets)</param>
        /// <returns></returns>
        public static string GetEffectSubPath(string fullPath)
        {
            string path = fullPath.Replace(SkillDefine.EffectPath, "");
            path = path.Replace(".prefab", "");
            return path;
        }
    }
}
