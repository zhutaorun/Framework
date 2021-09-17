using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// Camera Projection is Perspective 透视摄像机
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        //移动端缩放因子
        private const int MobileScaleFactor = 10;

        [Header("视角缩放因子")]
        public float fovSizeFactor = 10f;
        [Header("默认视野张角")]
        public float defaultFov = 80f;
        [Header("最大视野张角")]
        public float maxFov = 120f;
        [Header("最小视野张角")]
        public float minFov = 40f;
        [Header("相机与物体截面之间的距离")]
        public float cameraDistance = 100f;
        [Header("视野范围矩阵")]
        public Rect area;

        //主相机
        public new Camera camera { get; private set; }
        //输入
        private InputBase input;

        //边界
        private Vector2 leftDownBound = Vector2.zero;
        private Vector2 rightUpBound = Vector2.zero;
        //拖拽
        private Vector3 dragCacheValue = Vector3.zero;
        private Vector2 dragScale = Vector2.one;

        //缩放
        public float UIScale { get; private set; } = 1;

        //是否激活
        public bool EnbleInteract { get; set; } = true;

        private void OnEnable()
        {
            Instance = this;
            camera = GetComponent<Camera>();
            camera.fieldOfView = defaultFov;

            if(Application.isMobilePlatform)
            {
                if (input == null)
                    input = new TouchInput();
                input.Initialize(camera,MobileScaleFactor);
            }
            else
            {
                if (input == null)
                    input = new MouseInput();
                input.Initialize(camera, 1);
            }

            input.onDrag += OnDrag;
            input.onScale += OnScale;
            input.onDoubleClick += OnDoubleClick;

            OnScale(0.1f);
            CalculateBound();


        }

        public void OnDisable()
        {
            if(input!=null)
            {
                input.onDrag -= OnDrag;
                input.onScale -= OnScale;
                input.onDoubleClick -= OnDoubleClick;
            }
        }

        public void Update()
        {
            if (camera.enabled && EnbleInteract)
                input.Update();
        }

        private void OnScale(float scale)
        {
            if (Mathf.Approximately(scale, 0))
                return;

            //改变焦距
            var fov = camera.fieldOfView;
            fov = scale * fovSizeFactor;
            camera.fieldOfView = Mathf.Clamp(fov, minFov, maxFov);

            //重新计算边界
            CalculateBound();

            //处理边界
            var cameraPosition = camera.transform.position;
            var fX = Mathf.Clamp(cameraPosition.x,leftDownBound.x,rightUpBound.x);
            var fY = Mathf.Clamp(cameraPosition.y,leftDownBound.y,rightUpBound.y);
            camera.transform.position = new Vector3(fX,fY,cameraPosition.z);

            //UI放缩
            UIScale = Mathf.Tan(defaultFov * Mathf.Deg2Rad * 0.5f) / Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f);
        }

        private void OnDrag(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) < 0.01f && Mathf.Abs(delta.y) < 0.01f)
                return;
            OnDragCamera(delta);
        }

        private void OnDragCamera(Vector2 deltaPos)
        {
            dragCacheValue.Set(-deltaPos.x * dragScale.x, -deltaPos.y * dragScale.y, 0);

            Vector3 camPosition = camera.transform.position;

            var fX = camPosition.x + dragCacheValue.x;
            var fY = camPosition.y + dragCacheValue.y;

            //边界处理
            fX = Mathf.Clamp(fX, leftDownBound.x, rightUpBound.x);
            fY = Mathf.Clamp(fY, leftDownBound.y, rightUpBound.y);

            camera.transform.position = new Vector3(fX,fY,camPosition.z);
        }

        private void OnDoubleClick(Vector2 position)
        {
            var camPosition = camera.transform.position;
            var ray = camera.ScreenPointToRay(position);
            var distance = cameraDistance;

            //需要collider
            if(Physics.Raycast(ray,out var hit,2000f))
            {
                distance = hit.point.z - camPosition.z;
            }

            var newPos = camera.ScreenToWorldPoint(new Vector3(position.x,position.y,distance));

            //边界处理
            var fX = Mathf.Clamp(newPos.x, leftDownBound.x, rightUpBound.x);
            var fY = Mathf.Clamp(newPos.y, leftDownBound.y, rightUpBound.y);

            camera.transform.position = new Vector3(fX,fY,camPosition.z);
        }

        private void OnFocusPosition(Vector3 pos)
        {
            //聚焦
            camera.fieldOfView = minFov;
            //重新计算边界
            CalculateBound();
            //UI缩放
            UIScale = Mathf.Tan(defaultFov * Mathf.Deg2Rad * 0.5f) / Mathf.Tan(minFov * Mathf.Deg2Rad * 0.5f);

            //位移
            var cameraPosition = camera.transform.position;
            //边界处理
            var fX = Mathf.Clamp(pos.x, leftDownBound.x, rightUpBound.x);
            var fY = Mathf.Clamp(pos.y, leftDownBound.y, rightUpBound.y);
            camera.transform.position = new Vector3(fX,fY,cameraPosition.z);
        }

        public void CalculateBound()
        {
            var h = cameraDistance * Mathf.Abs(Mathf.Tan(camera.fieldOfView* Mathf.Deg2Rad* 0.5f));
            var rate = Screen.width / (float)Screen.height;
            float w = h * rate;

            dragScale.Set(w / Screen.width * 5f,h/Screen.height* 5f);
            leftDownBound.Set(area.xMin+w,area.yMin+h);
            rightUpBound.Set(area.xMax - w, area.yMax - h);
        }


    }
}
