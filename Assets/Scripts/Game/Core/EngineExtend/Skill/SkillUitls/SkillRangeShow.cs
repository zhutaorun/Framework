using SkillnewConfig;
using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SkillRangeShow : MonoBehaviour,IDisposable
{
    static SkillRangeShow _instance;
    private ShapeData currentShape;
    public event UnityEngine.Events.UnityAction OnDraw;
    public event UnityEngine.Events.UnityAction ResetFunc;

    public bool IsShowRange = true;

    public static SkillRangeShow Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject old = GameObject.Find("SkillRangeShow");
                if (old)
                    GameObject.DestroyImmediate(old);

                GameObject obj = new GameObject("SkillRangeShow");
                _instance = obj.AddComponent<SkillRangeShow>();
                if (Application.isPlaying)
                    GameObject.DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private bool show = true;

    private void OnGUI()
    {
        if(show)
        {
            if(GUILayout.Button("Clear"))
            {
                if (ResetFunc != null)
                    ResetFunc();
            }
        }
    }

    enum Shape
    {
        Empty,
        Rect,
        Ring,
        Circle,
        Sector,
    }

    struct ShapeData
    {
        public float width; //宽(矩形)
        public float height; //高(矩形)
        public float radius;//半径(圆,环)
        public float Radius;//大半径(环)
        public Vector3 center;//中心位置(圆,环，扇形，矩形)
        public Vector3 start;//起始点(矩形)
        public Vector3 end;//终点(矩形)
        public Vector3 dir;//方向(扇形)
        public float deg;//角度(扇形)
        public Shape shape;
        public Quaternion quat;
        public int drawFrameCount;
        public Mesh mesh;

        public void Reset()
        {
            width = 0;
            height = 0;
            radius = 0;
            Radius = 0;
            drawFrameCount = 1;
            center = Vector3.zero;
            start = Vector3.zero;
            end = Vector3.zero;
            deg = 0;
            dir = Vector3.zero;
            shape = default(Shape);
            quat = Quaternion.identity;
        }
        //矩形有两种表达方式:A:宽、高、中心位置、方向。B:起始点、终点、宽
    }

    private Queue<ShapeData> shapes = new Queue<ShapeData>();
    private Queue<ShapeData> pool = new Queue<ShapeData>();

    public void Reset()
    {
        while(shapes.Count>0)
        {
            ShapeData shape = shapes.Dequeue();
            shape.Reset();
            pool.Enqueue(shape);
        }
    }

    public void DrawCircle(Vector3 center,float radius,int lastFrame)
    {
        if (!IsShowRange) return;
        ShapeData shape = Get();
        shape.shape = Shape.Circle;
        shape.center = center;
        shape.radius = radius;
        shape.drawFrameCount = lastFrame;
        shapes.Enqueue(shape);
    }

    public void DrawRing(Vector3 center,float radius,float Radius,int lastFrame)
    {
        if (!IsShowRange) return;
        ShapeData shape = Get();
        shape.shape = Shape.Ring;
        shape.center = center;
        shape.radius = radius;
        shape.Radius = Radius;
        shape.drawFrameCount = lastFrame;
        shapes.Enqueue(shape);
    }

    public void DrawRect(Vector3 start, Vector3 end,float width, Quaternion quat, int lastFrame)
    {
        if (!IsShowRange) return;
        ShapeData shape = Get();
        shape.shape = Shape.Rect;
        shape.start = start;
        shape.end = end;
        shape.drawFrameCount = lastFrame;
        shape.width = width;
        shape.quat = quat;
        shapes.Enqueue(shape);
    }

    public void DrawSector(Vector3 center, Vector3 dir, float radius,float Radius,float deg, Quaternion quat, int lastFrame)
    {
        if (!IsShowRange) return;
        ShapeData shape = Get();
        shape.shape = Shape.Sector;
        shape.center = center;
        shape.radius = radius;
        shape.Radius = Radius;
        shape.deg = deg;
        shape.drawFrameCount = lastFrame;
        shape.dir = dir;
        shape.quat = quat;

        shapes.Enqueue(shape);
    }
   
    private Vector3 PrepareTargetPos(EPickCenterType centerType,GameObject self,Vector3 skillOffset)
    {
        switch(centerType)
        {
            case EPickCenterType.CmdPos:
            case EPickCenterType.LockTargetPos:
            case EPickCenterType.SelfPos:
                return self.transform.position + skillOffset;
        }
        return self.transform.position + skillOffset;
    }

    private Vector3 PrepareDr(EPickDirType dirType,GameObject self)
    {
        switch(dirType)
        {
            case EPickDirType.CmdDir:
            case EPickDirType.SelfDir:
            case EPickDirType.TargetDir:
                return -self.transform.forward;
        }
        return self.transform.forward;
    }

    private Quaternion PrepareQuaternion(EPickDirType dirType,GameObject self,float dirFrom,float dirTo)
    {
        float dirOffset = (dirFrom + dirTo) / 2;
        if (dirOffset == 0)
            return Quaternion.identity;
        else
        {
            return Quaternion.AngleAxis(dirOffset,self.transform.up);
        }
    }

    private Quaternion PrepareQuaternion(EPickDirType dirType,GameObject self,float dirOffset)
    {
        if (dirOffset == 0)
            return Quaternion.identity;
        else
        {
            return Quaternion.AngleAxis(dirOffset, self.transform.up);
        }
    }

    private float GetSkillRangeValue(ESkillRangeType rangeType,skillnew_data skill)
    {
        switch(rangeType)
        {
            case ESkillRangeType.AfterField:
                return skill.AttackRangeAfterFired * 0.01f;
            case ESkillRangeType.Range:
                return skill.SkillRange * 0.01f;
            case ESkillRangeType.RangeEx:
                return skill.AttackRangeEx * 0.01f;
            case ESkillRangeType.Redius:
                return skill.CollisoionRedius * 0.01f;
        }
        return 0;
    }

    private float GetSkillRangeValue(ESkillRangeType rangeType,skillnew_data skill,int attackRange)
    {
        if (attackRange <= 0 && skill != null)
            return GetSkillRangeValue(rangeType, skill);
        else
            return attackRange * 0.01f;
    }

    public void ShowRange(eventpick_data pick,skillnew_data skill,GameObject self,int lastFrame = 1)
    {
        if (!IsShowRange) return;
        if (pick == null) return;
        Vector3 skillOffset = new Vector3(-pick.EventCenterOffSetX* 0.01f,0,-pick.EventCenterOffSetZ* 0.01f);
        Vector3 offset_x = self.transform.right * skillOffset.x;
        Vector3 offset_z = self.transform.forward * skillOffset.z;
        skillOffset = offset_x + offset_z;

        Vector3 center = PrepareTargetPos(pick.EventPickCenterType,self,skillOffset);
        float radius = GetSkillRangeValue(pick.EventRangeMaxDistance,skill,pick.EventRangeMaxDistanceValue);
        float Radius = GetSkillRangeValue(pick.EventRangeMaxDistance,skill,pick.EventRangeMaxDistanceValue);
        Quaternion quat;
        switch(pick.EventPickShap)
        {
            case EPickShapType.Circle:
                if (radius > 0)
                    DrawRing(center, radius, Radius, lastFrame);
                else
                    DrawCircle(center,Radius,lastFrame);
                break;
            case EPickShapType.RectRangle:
                Vector3 start = PrepareTargetPos(pick.EventPickCenterType,self,skillOffset);
                Vector3 dir = PrepareDr(pick.EventPickDirType,self).normalized;
                quat = PrepareQuaternion(pick.EventPickDirType,self,pick.DirOffset);
                DrawRect(start + dir * GetSkillRangeValue(pick.EventRangeMinDistance,skill,pick.EventRangeMinDistanceValue),
                    start+dir * GetSkillRangeValue(pick.EventRangeMinDistance,skill,pick.EventRangeMaxDistanceValue),
                    GetSkillRangeValue(pick.EventRangeWidth,skill,pick.EventRangeWidthValue),
                    quat,
                    lastFrame);
                break;
            case EPickShapType.Sector:
                int angle = pick.EventAngleEnd - pick.EventAngleStart;
                dir = PrepareDr(pick.EventPickDirType,self).normalized;
                quat = PrepareQuaternion(pick.EventPickDirType,self,pick.EventAngleStart,pick.EventAngleEnd);
                DrawSector(center,dir,radius,Radius,angle,quat,lastFrame);
                break;
        }
    }


    List<ShapeData> cacheList = new List<ShapeData>();
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        cacheList.Clear();
        while(shapes.Count>0)
        {
            currentShape = shapes.Dequeue();
            DrawShape(currentShape);
            currentShape.drawFrameCount--;
            if(currentShape.drawFrameCount >0)
            {
                cacheList.Add(currentShape);
            }
            else
            {
                currentShape.Reset();
                pool.Enqueue(currentShape);
            }
        }
        foreach (var item in cacheList)
        {
            shapes.Enqueue(item);
        }
        if(OnDraw != null)
        {
            OnDraw();
        }
    }

    private void DrawShape(ShapeData data)
    {
        Debug.Log("DrawShape:" + data.shape);
        switch(data.shape)
        {
            case Shape.Circle:
                GizmosExtension.DrawCircleMesh(data.center,data.radius,data.mesh);
                break;
            case Shape.Rect:
                GizmosExtension.DrawRectangleMesh(data.center,data.start,data.end,data.width,data.quat,data.mesh);
                break;
            case Shape.Ring:
                GizmosExtension.DrawRingMesh(data.center,data.radius,data.Radius,data.mesh);
                break;
            case Shape.Sector:
                GizmosExtension.DrawSectorMesh(data.center,data.dir,data.radius,data.Radius,(int)data.deg,data.quat,data.mesh);
                break;
        }
    }

    private ShapeData Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();
        return new ShapeData();
    }

    public void Dispose()
    {
        if (this.gameObject)
            GameObject.DestroyImmediate(this.gameObject);
        _instance = null;
    }
}
