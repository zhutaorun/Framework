using BulletConfig;
using GameFrame.Skill.Component;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Utility
{
    public static class UtilityBullet
    {
        public static bool CreateBullet(int entityId,int bulletId,Vector3 offset,SkillTriggerParam param,Action<int,SkillTriggerParam> callback = null)
        {
            if(entityId<0)
            {
                Debug.LogError("CreateBullet error id not find!");
                return false;
            }
            bullet_data bullet = SkillController.SkillCfg.Get<bullet_data>(bulletId);
            if(bullet== null)
            {
                Debug.LogError("CreateBullet bulletId id not find!"+bulletId);
                return false;
            }
            Vector3 bulletDir = param.skillTriggerDir.normalized;
            switch(bullet.DirType)
            {
                case EBulletDirType.TargetDir:
                    if(param.skillCmdTarget != null)
                    {
                        Vector3 dir = (param.skillCmdTarget.transform.position - param.skillTriggerPos);
                        if(dir!= Vector3.zero)
                        {
                            dir = dir.normalized;
                            bulletDir = dir;
                        }
                    }
                    break;
                default:
                    break;
                    break;
            }
            Vector3 bulletPos = param.skillTriggerPos + Quaternion.LookRotation(bulletDir) * offset;
            ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransform>(entityId);
            trans.SelfControl = true;
            trans.RootGameObj = new GameObject("bullet"+entityId);
            trans.RootGameObjTransform = trans.RootGameObj.transform;
            trans.RootGameObjTransform.position = bulletPos;
            trans.RootGameObjTransform.rotation = Quaternion.LookRotation(bulletDir);
            UtilityEffect.PlayEffect(entityId,entityId,(int)bullet.EffectId);

            ComponentMove move = SkillController.Component.GetComponent<ComponentMove>(entityId);
            float dis = 0;
            if(bullet.LockTarget&& param.skillCmdTarget!= null)
            {
                dis = Vector3.Distance(bulletPos,param.skillCmdTarget.transform.position);
            }
            else
            {
                dis = bullet.BulletRangeValue / 100.0f;
            }
            move.SetBulletMoveLine(trans.RootGameObjTransform,dis,bullet.BulletSpeedValue/100.0f, bulletDir,()=> 
            {
                SkillController.ID.ReleaseModuleId(entityId);
                SkillController.Component.RemoveEntityComponent(entityId);
                if (callback != null)
                    callback(bulletId,param);
            });
            return true;
        }
    }
}