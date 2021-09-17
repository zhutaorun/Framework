using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    class InputBase : IUpdate
    {
        public event Action<Vector2> onDragStart;
        public event Action<Vector2> onDrag;
        public event Action<Vector2> onDragEnd;
        public event Action<float> onScale;
        public event Action<Vector2> onClick;
        public event Action<Vector2> onDoubleClick;
        public event Action<Transform> onGameObjectClick;


        public bool enableClick = true;
        public bool enableObjectClick = false;
        public float clickDistance = 100;

        protected bool isDragging = false;
        protected bool isPress = false;
        protected float scaleFactor = 1.0f;

        /// <summary>
        /// 双击间隔
        /// </summary>
        protected float doubleClickIntervalTime = 0.3f;

        /// <summary>
        /// 上次点击时间
        /// </summary>
        protected float lastClickTime = 0;

        /// <summary>
        /// 上次点击位置
        /// </summary>
        protected Vector2 lastPosition = Vector2.zero;

        protected Camera camera;
        protected List<RaycastResult> clickRaycastResult = new List<RaycastResult>();

        public Vector2 lasteScreenPosition = Vector2.zero;
        public Vector2 currentPos = Vector2.zero;

        public void Initialize(Camera camera,float scaleFactor = 1.0f)
        {
            this.camera = camera;
            this.scaleFactor = scaleFactor;
        }

        protected virtual void OnDragStart(Vector2 position)
        {
            onDragStart?.Invoke(position);
        }

        protected virtual void OnDrag(Vector2 delta)
        {
            onDrag?.Invoke(delta);
        }

        protected virtual void OnDragEnd(Vector2 position)
        {
            onDragEnd?.Invoke(position);
        }

        protected virtual void OnScale(float scale)
        {
            onScale?.Invoke(scale);
        }


        protected virtual void OnClick(Vector2 position)
        {
            if (!enableClick) return;
            float deltaTime = Time.realtimeSinceStartup - lastClickTime;
            if(deltaTime<= doubleClickIntervalTime)
            {
                if(Vector2.Distance(position,lastPosition)<0.1f)
                {
                    onDoubleClick?.Invoke(position);
                    lastClickTime = 0f;
                    lastPosition = Vector2.zero;
                    return;
                }
            }

            lastPosition = position;
            lastClickTime = Time.realtimeSinceStartup;
            onClick?.Invoke(position);
            if(enableObjectClick &&  camera!=null)
            {
                Ray ray = camera.ScreenPointToRay(position);
                if(Physics.Raycast(ray,out RaycastHit hitInfo,clickDistance))
                {
                    if(hitInfo.transform)
                    {
                        onGameObjectClick?.Invoke(hitInfo.transform);
                    }
                }
            }
        }

        protected bool IsClickUI()
        {
            if(EventSystem.current!=null)
            {
                PointerEventData eventData = null;
#if (UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                if(Input.touchCount==0)
                {
                    return false;
                }
#else
                if(!Input.GetMouseButton(0))
                {
                    return false;
                }
#endif

                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    return true;
                }
                else
                {
                    eventData = new PointerEventData(EventSystem.current);
#if (UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                    eventData.position = Input.GetTouch(0).position;
#else
                    eventData.position = Input.mousePosition;
#endif
                    clickRaycastResult.Clear();
                    EventSystem.current.RaycastAll(eventData, clickRaycastResult);

                    foreach(var raycastResult in clickRaycastResult)
                    {
                        if(raycastResult.gameObject.layer == Layer.UI.GetHashCode())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }



        public virtual void Update()
        {

        }
    }
}
