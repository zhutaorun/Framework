using GameFrame.Pool;
using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    public class ComponentSkill : PoolClass
    {
        public int CurSkillId { get; private set; }
        public skillnew_data CurSkillCfg;

        public int CurActNo { get; private set; }

        public float CurActStartPoint { get; private set; }
        public int CurSkillStartAct { get; private set; }
        public float CurActElapsedTime;
        public int CurActNextEventIdx;
        public bool ActionDirty;//动作有更新
        public float CurSkillDirWithMoveDirAngle;//当前技能与移动方向夹角
        public bool CheckMoveAction;//检查技能动作受移动影响数组

        public bool ActionChangeDirDirty;//动作换方向是否需要更新

        public string CurAnimName;

        public float CurActionSpeed;
        public int CurActionAnimIndex;
        public bool CameraFollowHeight;//相机高度跟随
        public Dictionary<int, int> SkillChangeDic;//引用

        public bool SkillStart = false;//是否技能开始
        public bool StopMove = false;//是否停止了移动
        public bool AutoNext = false;

        public SkillTriggerParam TriggerParam;//技能触发时参数

        public List<int> ForbidenSkillList;
        private bool _skillover = false;

        public bool SkillOver
        {
            get
            {
                return _skillover;
            }
        }

        public void SetSkillTriggerParam(GameObject self, GameObject target, int heroId, int targetId, Vector3 dir, Vector3 pos)
        {
            TriggerParam.SkillCfg = CurSkillCfg;
            TriggerParam.skillCmdDir = dir;
            TriggerParam.skillCmdPos = pos;
            TriggerParam.skillCmdTarget = target;
            TriggerParam.heroId = heroId;
            TriggerParam.skillCmdTargetId = targetId;
            TriggerParam.skillTriggerDir = self.transform.eulerAngles;
            TriggerParam.skillTriggerPos = self.transform.position;
        }


        public void SetSkillActionRefresh(int skillId, int actNo, float actStartPoint, float speed, float withMoveAngle, bool checkMoveAction, bool autoNext)
        {
            if (CurSkillId != skillId)
                CurSkillStartAct = actNo;
            CurSkillId = skillId;
            if (CurSkillId <= 0)
            {
                CurSkillCfg = null;
            }
            else
            {
                CurSkillCfg = SkillController.SkillCfg.Get<skillnew_data>(CurSkillId);
            }
            CurActNo = actNo;
            CurActElapsedTime = 0;
            CurActNextEventIdx = 0;
            CurActionSpeed = speed;
            CurSkillDirWithMoveDirAngle = withMoveAngle;
            SkillStart = true;
            CheckMoveAction = checkMoveAction;
            CurActStartPoint = actStartPoint;
            ActionDirty = true;
            if (checkMoveAction)
                ActionChangeDirDirty = true;
            StopMove = false;
            AutoNext = autoNext;
            _skillover = false;
            if (skillId <= 0)
            {
                ResetState();
            }
        }


        public void ForceOver()
        {
            _skillover = true;
        }

        /// <summary>
        /// 重置一些状态变量
        /// </summary>
        private void ResetState()
        {
            CameraFollowHeight = false;
        }

        public void ChangeSkillActionWithDir(int skillId, float withMoveAngle, bool stopMove)
        {
            if (skillId != CurSkillId)
                return;
            CurSkillDirWithMoveDirAngle = withMoveAngle;
            StopMove = stopMove;
            SkillStart = false;
            CheckMoveAction = true;
            ActionChangeDirDirty = true;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            CurSkillId = 0;
            CurSkillCfg = null;
            CurActNo = 0;
            CurActStartPoint = 0;
            CurSkillStartAct = 0;
            CurActElapsedTime = 0;
            CurActNextEventIdx = 0;
            ActionDirty = false;
            CurSkillDirWithMoveDirAngle = 0;
            CheckMoveAction = false;
            ActionChangeDirDirty = false;
            CurAnimName = string.Empty;
            CurActionSpeed = 1.0f;
            SkillChangeDic = null;
            CurActionAnimIndex = -1;
            StopMove = false;
            SkillStart = false;
            ForbidenSkillList = null;
            AutoNext = false;
            TriggerParam = default(SkillTriggerParam);
        }
    }

    public struct SkillTriggerParam
    {
        public SkillnewConfig.skillnew_data SkillCfg;

        /// <summary>
        /// 技能选择的方向
        /// </summary>
        public Vector3 skillCmdDir;

        /// <summary>
        /// 技能选择的坐标
        /// </summary>
        public Vector3 skillCmdPos;

        /// <summary>
        /// 技能施法者id
        /// </summary>
        public int heroId;

        /// <summary>
        /// 技能选中的targetEntityId
        /// </summary>
        public int skillCmdTargetId;

        /// <summary>
        /// 技能选中的target
        /// </summary>
        public GameObject skillCmdTarget;

        /// <summary>
        /// 技能触发时的方向
        /// </summary>
        public Vector3 skillTriggerDir;

        /// <summary>
        /// 技能触发时的坐标
        /// </summary>
        public Vector3 skillTriggerPos;

        /// <summary>
        /// 扩展参数
        /// </summary>
        public object extraParam;
    }
}