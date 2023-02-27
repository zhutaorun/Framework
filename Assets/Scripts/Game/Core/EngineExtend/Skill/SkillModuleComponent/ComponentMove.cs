using GameFrame.Pool;
using GameFrame.Skill.Utility;
using SkillmoveConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill.Component
{
    public class ComponentMove : PoolClass
    {
        public Action MoveOverCallback;
        public float MoveRemainDistance;
        public float CurMoveSpeed;
        public float VerticalStartSpeed;
        public float VerticalAcc;
        public bool VerticalMoveOver;
        public bool LineMoveOver;
        public Vector3 MoveDir;
        public bool IsMoving;
        public float ElapseTime;

        public void SetBuffMoveCmdData(Transform target,skillmove_data movecfg,float distance,Vector3 dir,float speed,float height,int verticalTime,Action moveoverCb,SkillTriggerParam fromskill = default(SkillTriggerParam))
        {
            if(IsMoving)
            {
                MoveOverCallback?.Invoke();
            }
            clearMove();
            Vector3 moveDir = dir;
            if (moveDir != Vector3.zero)
                moveDir = dir.normalized;

            float moveDistance = distance;
            //根据高度和距离算初速度和加速度
            float startPosY = target.position.y + height - SkillDefine.MAP_FLOOR;
            float verticalAcc = 0;
            if(startPosY>0)
            {
                float fallTime = 0;
                float vStartSpeed = UtilityCommon.GetVerticalStartSpeed(height,ref verticalAcc,verticalTime);
                if (verticalAcc == 0 || startPosY == 0)
                {
                    fallTime = verticalTime / 1000.0f / 2;
                }
                else
                    fallTime = Mathf.Sqrt(startPosY* 2/verticalAcc);
                float lineMoveTime = 0;
                if (distance > 0 && speed > 0)
                    lineMoveTime =  distance / speed;
                float verticaltime = verticalTime / 1000.0f / 2 + fallTime;
                if (lineMoveTime < verticaltime)
                    moveDistance = verticaltime * speed;
            }
            Vector3 validPos = Vector3.zero;
            if (moveDistance < 0.1f)
                moveDistance = 0;

            MoveOverCallback = moveoverCb;
            MoveRemainDistance = moveDistance;
            CurMoveSpeed = speed;
            VerticalStartSpeed = UtilityCommon.GetVerticalStartSpeed(height,ref verticalAcc,verticalTime);
            VerticalAcc = verticalAcc;
            VerticalMoveOver = false;
            LineMoveOver = false;
            if(height == 0)
            {
                if(Mathf.Abs(target.position.y - SkillDefine.MAP_FLOOR)<0.1f)
                {
                    VerticalMoveOver = true;
                    target.position = new Vector3(target.position.x, SkillDefine.MAP_FLOOR, target.position.z);
                }
                else
                {
                    VerticalStartSpeed = 0;
                    VerticalAcc = 40;//默认加速度
                }
            }

            SetMovingDir(dir);
            if(!movecfg.KeepDir)
            {
                target.rotation = Quaternion.LookRotation(dir);
            }
            IsMoving = true;
        }
            
        public void SetBulletMoveLine(Transform target,float distance,float speed,Vector3 dir,Action moveOverCb)
        {
            clearMove();
            SetMovingDir(dir);
            CurMoveSpeed = speed;
            IsMoving = true;
            VerticalMoveOver = true;
            MoveOverCallback = moveOverCb;
            MoveRemainDistance = distance;
        }

        public void SetMovingDir(Vector3 dir)
        {
            if(dir == Vector3.zero)
            {
                return;
            }
            dir = dir.normalized;
            MoveDir = dir;
        }

        void clearMove()
        {
            MoveOverCallback = null;
            MoveRemainDistance = 0;
            CurMoveSpeed = 0;
            VerticalStartSpeed = 0;
            VerticalAcc = 0;
            VerticalMoveOver = false;
            LineMoveOver = false;
            MoveDir = default(Vector3);
            IsMoving = false;
            ElapseTime = 0;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            clearMove();
        }
    }
}