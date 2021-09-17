using DG.Tweening;
using UnityEngine;
using System;
namespace GameFrame
{
    /// <summary>
    /// 地图边界定义
    /// </summary>
    [Serializable]
    public struct MapBound
    {
        public float Left;
        public float Right;
        public float Up;
        public float Down;
    }

    /// <summary>
    /// Camera Projection is Orthographic 正交摄像机
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Camera2DFollow:MonoBehaviour
    {

        public static Camera2DFollow Instance { get; private set; }

        /// <summary>
        /// 地图边界
        /// </summary>
        public MapBound MapBound;
        /// <summary>
        /// 地图坐标
        /// </summary>
        public Vector3 MapPos;

        /// <summary>
        /// 最小Z值
        /// </summary>
        public float MinOrthographicSize = 5.0f;
        /// <summary>
        /// 默认Z值
        /// </summary>
        public float DefaultOrthographisSize = 10.0f;
        /// <summary>
        /// 最大Z值
        /// </summary>
        public float MaxOrthographisSize = 20.0f;
        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed = 10f;
        /// <summary>
        /// 聚焦移动速度
        /// </summary>
        public float FocusSpeed = 10f;
        /// <summary>
        /// 镜头视角拉近远的速度
        /// </summary>
        public float ZoomSpeed = 10f;
        /// <summary>
        /// 摄像机对象
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// 输入模块接口
        /// </summary>
        private InputBase input;

        /// <summary>
        /// 当前拖动所在屏幕坐标
        /// </summary>
        private Vector2 currentDragPosition;

        /// <summary>
        /// 指定了拖动的地面为世界坐标Z=0平面
        /// </summary>
        private float MapZAxis = 0.0f;
        private Vector3 MapScreenPoint;
        /// <summary>
        /// 当前摄像机OrthographicSize
        /// </summary>
        private float currentCameraOrthographicSize = 0.0f;

        private void OnEnable()
        {
            Instance = this;
            MainCamera = GetComponent<Camera>();
            if(Application.isMobilePlatform)
            {
                if (input == null)
                {
                    input = new TouchInput();
                }
                else
                {
                    if (input == null)
                        input = new MouseInput();
                }


                input.Initialize(MainCamera);
                input.onDragStart += OnDragStart;
                input.onDrag += OnDrag;
                input.onScale += OnScale;
                input.onDoubleClick += OnDoubleClick;

                MapScreenPoint = MainCamera.WorldToScreenPoint(new Vector3(0.0f, 0.0f, MapZAxis));

                SetCameraOrthographicSize(DefaultOrthographisSize);

            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.onDragStart -= OnDragStart;
                input.onDrag -= OnDrag;
                input.onScale -= OnScale;
                input.onDoubleClick -= OnDoubleClick;
            }
        }


        public void Update()
        {
            if(MainCamera!=null && MainCamera.enabled)
            {
                input.Update();
                SmoothScale();
            }
        }

        private void OnDragStart(Vector2 position)
        {
            currentDragPosition = position;
        }

        private void OnDrag(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) < 0.01f && Mathf.Abs(delta.y) < 0.01f)
            {
                return;
            }

            //计算下个位置，屏幕显示范围世界坐标
            var leftDownPos = MainCamera.ScreenToWorldPoint(-delta);
            var rightTopPos = MainCamera.ScreenToWorldPoint(new Vector2(Screen.width - delta.x, Screen.height - delta.y));

            //调整
            if (leftDownPos.y < MapBound.Left + MapPos.x || rightTopPos.x > MapBound.Right + MapPos.x)
            {
                delta.x = 0;
            }
            if (leftDownPos.y < MapBound.Down + MapPos.y || rightTopPos.y > MapBound.Up + MapPos.y)
            {
                delta.y = 0;
            }
            if (Mathf.Abs(delta.x) < 0.01f && Mathf.Abs(delta.y) < 0.01f)
            {
                return;
            }

            float canVertExtent = MainCamera.orthographicSize;
            float canHoriExtent = MainCamera.orthographicSize * MainCamera.aspect;

            //移动
            var lastPos = MainCamera.ScreenToWorldPoint(new Vector3(currentDragPosition.x, currentDragPosition.y, MapScreenPoint.z));
            currentDragPosition += delta;
            var currentPos = MainCamera.ScreenToWorldPoint(new Vector3(currentDragPosition.x, currentDragPosition.y, MapScreenPoint.z));
            delta = currentPos - lastPos;

            Vector3 cameraPos = MainCamera.transform.position;
            Vector3 newPos = new Vector3(cameraPos.x - delta.x, cameraPos.y - delta.y, cameraPos.z);
            MainCamera.transform.DOMove(newPos, Time.timeScale / MoveSpeed);
        }

        private void OnScale(float scale)
        {
            if (scale == 0)
                return;

            float tmpSize = currentCameraOrthographicSize * (1 + scale);
            if(tmpSize>MaxOrthographisSize)
            {
                tmpSize = MaxOrthographisSize;
            }

            if (tmpSize < MinOrthographicSize)
                tmpSize = MinOrthographicSize;

            SetCameraOrthographicSize(tmpSize);
        }

        private void OnDoubleClick(Vector2 position)
        {
            Vector2 delta = new Vector2(Screen.width/2-position.x,Screen.height/2- position.y);

            //计算下个位置，屏幕显示范围世界坐标
            var leftDownPos = MainCamera.ScreenToWorldPoint(-delta);
            var rightTopPos = MainCamera.ScreenToWorldPoint(new Vector2(Screen.width - delta.x, Screen.height - delta.y));

            //调整
            if (leftDownPos.y < MapBound.Left + MapPos.x || rightTopPos.x > MapBound.Right + MapPos.x)
            {
                delta.x = 0;
            }
            if (leftDownPos.y < MapBound.Down + MapPos.y || rightTopPos.y > MapBound.Up + MapPos.y)
            {
                delta.y = 0;
            }
            if (Mathf.Abs(delta.x) < 0.01f && Mathf.Abs(delta.y) < 0.01f)
            {
                return;
            }

            var currentPos = MainCamera.ScreenToWorldPoint(new Vector3(position.x,position.y,MapScreenPoint.z));
            Vector3 cameraPos = MainCamera.transform.position;
            Vector3 newPos = new Vector3(currentPos.x,currentPos.y,cameraPos.z);
            MainCamera.transform.DOMove(newPos,Time.timeScale/FocusSpeed);
        }


        private void OnFocusPosition(Vector3 pos)
        {
            MapBound newBound = new MapBound();

            newBound.Left = MapPos.x + MapBound.Left;
            newBound.Right = MapPos.x + MapBound.Right;
            newBound.Up = MapPos.y + MapBound.Up;
            newBound.Down = MapPos.y + MapBound.Down;
            float verticalSize = MainCamera.orthographicSize;
            float horizonalSize = MainCamera.orthographicSize * MainCamera.aspect;

            float minCameraPosx = newBound.Left + horizonalSize;
            float maxCameraPosx = newBound.Right - horizonalSize;
            float minCameraPosy = newBound.Down + verticalSize;
            float maxCameraPosy = newBound.Up - verticalSize;

            float newCameraPosx = pos.x;
            float newCameraPosy = pos.y;
            float newCameraPosz = MainCamera.transform.position.z;

            if (newCameraPosx < minCameraPosx)
                newCameraPosx = minCameraPosx;
            if (newCameraPosx > maxCameraPosx)
                newCameraPosx = maxCameraPosx;
            if (newCameraPosy < minCameraPosy)
                newCameraPosy = minCameraPosy;
            if (newCameraPosy > maxCameraPosy)
                newCameraPosy = maxCameraPosy;
            MainCamera.transform.DOMove(new Vector3(newCameraPosx, newCameraPosy, newCameraPosz), 1.0f);
           
        }

        private void SetCameraOrthographicSize(float value)
        {
            float bakValue = MainCamera.orthographicSize;

            MainCamera.orthographicSize = value;
            Vector3 cameraPos = MainCamera.transform.position;

            //计算下个位置，屏幕显示范围世界坐标
            var leftDownPos = MainCamera.ScreenToWorldPoint(Vector2.zero);
            var rightTopPos = MainCamera.ScreenToWorldPoint(new Vector2(Screen.width,Screen.height));

            //计算完成，恢复
            MainCamera.orthographicSize = bakValue;

            //判断
            if (leftDownPos.x < MapBound.Left + MapPos.x || rightTopPos.x > MapBound.Right + MapPos.x)
                return;
            if (leftDownPos.y < MapBound.Down + MapPos.y || rightTopPos.y > MapBound.Up + MapPos.y)
                return;

            currentCameraOrthographicSize = value;
        }

        private void SmoothScale()
        {
            if(MainCamera!=null)
            {
                MainCamera.orthographicSize = Mathf.Lerp(MainCamera.orthographicSize,currentCameraOrthographicSize,Time.timeScale* ZoomSpeed);
            }
        }


        private void OnCameraFocusPositionEvent(Vector3 pos)
        {
            OnFocusPosition(pos);
        }
    }
}
