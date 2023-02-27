using GameFrame.Skill.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SkillInterface
{
    #region 必须调用

    /// <summary>
    /// 调用加载配置初始化(使用所有技能前调用)
    /// </summary>
    void Init();


    /// <summary>
    /// 释放(副本结束调用)
    /// </summary>
    void Release();

    /// <summary>
    /// 调用更新
    /// </summary>
    /// <param name="deltime"></param>
    void Update(float deltime);

    /// <summary>
    /// 删除entity,回收前调用
    /// </summary>
    /// <param name="entity"></param>
    void OnEntityRemove(int entity);
    #endregion

    /// <summary>
    /// 播放技能
    /// </summary>
    /// <param name="self">当前播放技能的角色GameObject</param>
    /// <param name="entityId">当前播放技能的角色entityId</param>
    /// <param name="skillId">技能ID</param>
    /// <param name="target">目标</param>
    /// <param name="targetEntityId">目标entityId</param>
    /// <param name="dir">技能选择的方向</param>
    /// <param name="pos">技能选择的坐标</param>
    /// <param name="actNo">技能动作序号</param>
    /// <param name="point">技能动作起始帧</param>
    /// <param name="antoNext">是否自动播放下面的动作</param>
    /// <param name="speed"></param>
    /// <param name="unitScale"></param>
    void PlaySkill(GameObject self,int entityId,int skillId,GameObject target =null,int targetEntityId = 0,Vector3 dir =default(Vector3),Vector3 pos = default(Vector3),int actNo =0,int point =0,bool antoNext = true,float speed =1.0f,float unitScale = 1.0f);


    /// <summary>
    /// 播放Buff显示
    /// </summary>
    /// <param name="entityId">buff的entityId</param>
    /// <param name="attchEntityId">buff目标的entityId</param>
    /// <param name="attachEntity">buff目标的HGameObject</param>
    /// <param name="buffId">buff的配置表Id</param>
    /// <param name="triggerSkillParam">buff来自技能的参数</param>
    void PlayBuff(int entityId, int attchEntityId, GameObject attachEntity, int buffId, SkillTriggerParam triggerSkillParam = default(SkillTriggerParam));


    /// <summary>
    /// buff结束调用
    /// </summary>
    /// <param name="entityId">buff的entityId</param>
    /// <param name="buffId">buff的配置表id</param>
    void BuffOver(int entityId,int buffId);

    /// <summary>
    /// 播放陷阱
    /// </summary>
    /// <param name="entityId">陷阱的entityId</param>
    /// <param name="trapId">陷阱的配置表id</param>
    /// <param name="pos">陷阱的初始坐标</param>
    /// <param name="dir">陷阱的初始方向</param>
    void PlayTrap(int entityId, int trapId, Vector3 pos, Vector3 dir);


    /// <summary>
    /// 播放子弹显示
    /// </summary>
    /// <param name="entityId">子弹的entityId(考虑删除这个参数，完全内部创建)</param>
    /// <param name="bulletId">子弹的配置id</param>
    /// <param name="offse">子弹偏移坐标相对于0，0，1方向</param>
    /// <param name="skillParam">子弹来自技能的参数</param>
    /// <param name="callback"></param>
    void PlayBullet(int entityId, int bulletId, Vector3 offse, SkillTriggerParam skillParam, System.Action<int, SkillTriggerParam> callback = null);

    /// <summary>
    /// 播放子弹显示
    /// </summary>
    /// <param name="bulletId">子弹的配置id</param>
    /// <param name="offse">子弹偏移坐标相对于0，0，1方向</param>
    /// <param name="skillParam">子弹来自技能的参数</param>
    /// <param name="callback"></param>
    void PlayBullet(int bulletId, Vector3 offse, SkillTriggerParam skillParam, System.Action<int, SkillTriggerParam> callback = null);

    /// <summary>
    /// 对指定目标播放特效
    /// </summary>
    /// <param name="target"></param>
    /// <param name="entityId"></param>
    /// <param name="effectId"></param>
    /// <param name="offset"></param>
    /// <param name="targetScale"></param>
    uint PlayEntityEffect(GameObject target,int entityId,int effectId,Vector3 offset = default(Vector3),float targetScale = 1.0f);


    /// <summary>
    /// 播放地图特效
    /// </summary>
    /// <param name="effectId">特效的配置id</param>
    /// <param name="pos">地图坐标</param>
    /// <param name="dir">特效方向</param>
    uint PlayWorldEffect(int effectId,Vector3 pos,Vector3 dir);


    /// <summary>
    /// 移除指定目标的特效
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="effectUid"></param>
    void EntityEffectOver(int entityId,uint effectUid);

    // <summary>
    /// 移除地图特效
    /// </summary>
    /// <param name="effectId">特效的配置id,创建特效时的返回值</param>
    void WorldEffectOver(uint effectId);


    /// <summary>
    /// 更新entity坐标(一般用于更新由技能模块创建的模型)
    /// </summary>
    /// <param name="entityId">需要更新的entityId</param>
    /// <param name="pos">当前坐标</param>
    void UpdateEntityPos(int entityId, Vector3 pos);


    /// <summary>
    /// 播放动作
    /// </summary>
    /// <param name="target">需要播放动作的entity的GameObject</param>
    /// <param name="entityId">需要播放动作的entityId</param>
    /// <param name="actionName">动作名(SkillDefine.AnimationPath后路径)</param>
    /// <param name="fadeTime">动画融合时间</param>
    /// <param name="speed">动画播放速度</param>
    void PlayAction(GameObject target, int entityId, string actionName, float fadeTime, float speed = 1.0f);
}
