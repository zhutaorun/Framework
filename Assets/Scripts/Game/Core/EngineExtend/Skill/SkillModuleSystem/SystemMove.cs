using GameFrame.Skill.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Skill.GameSystem
{
    public class SystemMove : ISystem
    {
        public void Update(float delTime)
        {
            ComponentMove moveComponent;
            Component<ComponentMove> components = SkillController.Component.GetComponents<ComponentMove>();
            if(components != null)
            {
                for(int i = 0, imax= components.DataCount;i<imax ; i++)
                {
                    moveComponent = components.GetComponent(i);
                    if (moveComponent == null)
                        continue;

                    OnUpdate(i,moveComponent,delTime);
                }
            }
        }

        void OnUpdate(int entity,ComponentMove moveComponent,float delTime)
        {
            if (!moveComponent.IsMoving)
                return;
            ComponentTransform trans = SkillController.Component.GetComponent<ComponentTransform>(entity);
            if (trans.RootGameObjTransform == null)
                return;
            moveComponent.ElapseTime += delTime;
            if(!moveComponent.LineMoveOver)
            {
                float speed = moveComponent.CurMoveSpeed;
                float moveDistance = speed * delTime;
                if(moveComponent.MoveRemainDistance <= moveDistance)
                {
                    moveDistance = moveComponent.MoveRemainDistance;
                    moveComponent.LineMoveOver = true;
                }
                moveComponent.MoveRemainDistance -= moveDistance;
                Vector3 moveDis = moveComponent.MoveDir * moveDistance;
                Vector3 newPos = trans.RootGameObjTransform.position + moveDis;
                trans.RootGameObjTransform.position = newPos;
            }
            if(!moveComponent.VerticalMoveOver)
            {
                float speed = moveComponent.VerticalStartSpeed - moveComponent.VerticalAcc * moveComponent.ElapseTime;
                float disY = speed * delTime;
                float newY = trans.RootGameObjTransform.position.y + disY;
                if(newY < SkillDefine.MAP_FLOOR)
                {
                    newY = SkillDefine.MAP_FLOOR;
                    moveComponent.VerticalMoveOver = true;
                }
                trans.RootGameObjTransform.position = new Vector3(trans.RootGameObjTransform.position.x,newY,trans.RootGameObjTransform.position.z);
            }

            if(moveComponent.VerticalMoveOver && moveComponent.LineMoveOver)
            {
                moveComponent.IsMoving = false;
                moveComponent.MoveOverCallback?.Invoke();
            }

        }
    }
}
