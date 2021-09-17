using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    class MouseInput : InputBase
    {
        public MouseInput()
        {

        }

        public override void Update()
        {
            HandleMouse();
        }

        private void HandleMouse()
        {
            if (IsClickUI()) return;

            if(Input.GetMouseButton(0))
            {
                this.lasteScreenPosition = Input.mousePosition;
                this.currentPos = Input.mousePosition;
            }

            if(!Input.GetMouseButton(0))
            {
                if(this.isDragging)
                {
                    OnDragEnd(Input.mousePosition);
                }
                else if(this.isPress)
                {
                    OnClick(Input.mousePosition);
                }
                this.isDragging = false;
                this.isPress = false;
            }
            else
            {
                if(this.isDragging)
                {
                    Vector2 currentPosition = (Vector2)Input.mousePosition;
                    OnDrag(currentPosition - this.lasteScreenPosition);
                    lasteScreenPosition = currentPosition;
                }
                else
                {
                    if(!this.isPress)
                    {
                        this.lasteScreenPosition = Input.mousePosition;
                        this.isPress = true;
                    }

                    if(this.isPress && Vector2.Distance(Input.mousePosition,this.lasteScreenPosition)>0.1f)
                    {
                        this.isDragging = true;
                        OnDragStart(this.lasteScreenPosition);
                    }
                }
            }

            float fValue = -Input.GetAxis("Mouse ScrollWheel");
            OnScale(fValue);
        }
    }
}
