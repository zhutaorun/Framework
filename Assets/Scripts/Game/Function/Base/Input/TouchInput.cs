using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    class TouchInput : InputBase
    {
        /// <summary>
        /// key:FingerId
        /// value :Index
        /// </summary>
        private Dictionary<int, int> m_FingerIDsIndex = new Dictionary<int, int>();
        private Dictionary<int, Touch> m_FingersDownInfo = new Dictionary<int, Touch>();    //手指首次按下对应点信息
        private Dictionary<int, Touch> m_FingersUpInfo = new Dictionary<int, Touch>();      //手指离开对应点信息
        private Dictionary<int, Touch> m_FingersInfo = new Dictionary<int, Touch>();        //所有触屏手指信息

        private bool isBeginTwoTouchs = false;
        private Vector2 lastTouchPosition1 = Vector2.zero;
        private Vector2 lastTouchPosition2 = Vector2.zero;
        private Touch m_UnValidTouch = new Touch();

        public TouchInput()
        {
            m_UnValidTouch.fingerId = -1;
        }

        public override void Update()
        {
            int index = 0;
            for(int i=0;i<Input.touchCount;i++)
            {
                Touch touch = Input.GetTouch(i);
                switch(touch.phase)
                {
                    case TouchPhase.Began:
                        index = m_FingersInfo.Count;

                        m_FingersDownInfo[index] = touch;
                        m_FingersInfo[index] = touch;
                        m_FingerIDsIndex[touch.fingerId] = index;
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        if(m_FingerIDsIndex.TryGetValue(touch.fingerId,out index))
                        {
                            m_FingersUpInfo[index] = touch;
                            m_FingersInfo[index] = touch;
                            m_FingerIDsIndex.Remove(touch.fingerId);
                        }
                        break;
                    case TouchPhase.Moved:
                        if(m_FingerIDsIndex.TryGetValue(touch.fingerId,out index))
                        {
                            m_FingersInfo[index] = touch;
                        }
                        break;
                }
            }

            if(Input.touchCount==0)
            {
                ClearFingersInfo();
            }
            HandleTouche();
        }

        private void HandleTouche()
        {
            if (IsClickUI()) return;
            if (IsSingleFirstFingerTouch())//仅首指触屏
            {
                Vector2 newScreenPos = GetFingersInfo(0).position;
                if (!isDragging)
                {
                    if (this.isPress)
                    {
                        if (Vector2.Distance(newScreenPos, this.lasteScreenPosition) > 0.1f)
                        {
                            this.isDragging = true;
                            OnDragStart(this.lasteScreenPosition);
                        }
                    }

                    if (!isPress)
                    {
                        this.lasteScreenPosition = newScreenPos;
                        this.isPress = true;
                    }
                }
                else
                {
                    Vector2 currentPosition = newScreenPos;
                    OnDrag(currentPosition - this.lasteScreenPosition);
                    this.lasteScreenPosition = currentPosition;
                }
            }
            else
            {
                if(this.isDragging)
                {
                    OnDragEnd(GetFingerUpInfo(0).position);
                }
                else if(this.isPress)
                {
                    OnClick(GetFingerUpInfo(0).position);
                }
            }

            this.isDragging = false;
            this.isPress = false;

            if(IsSinglePreTwoFingersTouch())//仅前双指触屏
            {
                if(!isBeginTwoTouchs)
                {
                    isBeginTwoTouchs = true;
                    this.lastTouchPosition1 = GetFingersInfo(0).position;
                    this.lastTouchPosition2 = GetFingersInfo(1).position;
                }
            }
            else
            {
                isBeginTwoTouchs = false;
            }

            //手势缩放
            if(isBeginTwoTouchs)
            {
                Vector3 tempPosition1 = GetFingersInfo(0).position;
                Vector3 tempPosition2 = GetFingersInfo(1).position;

                float fScale = CalculateTouchsScale(lastTouchPosition1, lastTouchPosition2, tempPosition1, tempPosition2);

                if(Mathf.Abs(fScale)>0.0001f)
                {
                    OnScale(fScale * scaleFactor);
                }

                lastTouchPosition1 = tempPosition1;
                lastTouchPosition2 = tempPosition2;
            }
        }


        /// <summary>
        /// 计算两指滑动缩放系数
        /// </summary>
        /// <param name="oP1"></param>
        /// <param name="oP2"></param>
        /// <param name="nP1"></param>
        /// <param name="nP2"></param>
        /// <returns></returns>
        public float CalculateTouchsScale(Vector2 oP1,Vector2 oP2,Vector2 nP1,Vector2 nP2)
        {
            //函数传入上次一次触摸两点的位置与本次触摸两点位置的位置计算出用户的手势
            float oldLength = Vector2.Distance(oP1,oP2);
            float newLength = Vector2.Distance(nP1,nP2);
            return (oldLength - newLength) / Screen.width;
        }

        public bool IsSingleFirstFingerTouch()
        {
            return m_FingersInfo.Count == 1 && m_FingersInfo[0].fingerId != m_UnValidTouch.fingerId;
        }

        public bool IsSinglePreTwoFingersTouch()
        {
            return m_FingersInfo.Count == 2 && m_FingersInfo[0].fingerId != m_UnValidTouch.fingerId && m_FingersInfo[1].fingerId != m_UnValidTouch.fingerId;
        }

        private Touch GetFingersInfo(int index)
        {
            Touch touch;
            m_FingersInfo.TryGetValue(index,out touch);
            return touch;
        }

        private Touch GetFingerUpInfo(int index)
        {
            Touch touch;
            m_FingersUpInfo.TryGetValue(index,out touch);
            return touch;
        }

        /// <summary>
        /// 清空所有信息
        /// </summary>
        private void ClearFingersInfo()
        {
            m_FingerIDsIndex.Clear();
            m_FingersInfo.Clear();
            m_FingersDownInfo.Clear();
            m_FingersUpInfo.Clear();
        }
    }
}
