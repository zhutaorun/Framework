using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosExtension
{
    private static int meshSize = 5;

    public static void DrawCirCle(Vector3 center, float radius)
    {
        float deg = 0;
        Vector3 lastpoint = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radius + center.x, center.y, Mathf.Sin(deg * Mathf.Deg2Rad) * radius + center.z);
        Vector3 newpoint = Vector3.zero;
        while (deg < 360)
        {
            newpoint.Set(Mathf.Cos(deg*Mathf.Deg2Rad)* radius+center.x,center.y,Mathf.Sin(deg*Mathf.Deg2Rad)* radius+center.z);
            Gizmos.DrawLine(lastpoint,newpoint);
            deg += 1;
            lastpoint.Set(newpoint.x,newpoint.y,newpoint.z);
        }
    }

    public static void DrawUICircle(Transform trans,Vector3 center,float m_Radius,Color m_Color)
    {
        float m_Theta = 0.1f;
        if (m_Theta < 0.0001f) m_Theta = 0.0001f;

        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = trans.localToWorldMatrix;

        Color defaultColor = Gizmos.color;
        Gizmos.color = m_Color;

        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        for(float theta =0;theta<2* Mathf.PI;theta += m_Theta)
        {
            float x = m_Radius * Mathf.Cos(theta) + center.x;
            float y = m_Radius * Mathf.Sin(theta) + center.y;
            Vector3 endPoint = new Vector3(x,y,0);
            if(theta == 0)
            {
                firstPoint = endPoint;
            }
            else
            {
                Gizmos.DrawLine(beginPoint,endPoint);
            }
        }

        Gizmos.DrawLine(firstPoint,beginPoint);
        Gizmos.color = defaultColor;
        Gizmos.matrix = defaultMatrix;
    }


    public static void DrawCircleMesh(Vector3 center,float radius,Mesh mesh)
    {
        if(mesh == null)
        {
            mesh = new Mesh();
            mesh.Clear();
            float deg = 0;
            Vector3 lastpoint = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radius,0,Mathf.Sin(deg* Mathf.Deg2Rad)* radius);
            Vector3 newpoint = Vector3.zero;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            vertices.Add(newpoint);

            while(deg <= 360)
            {
                newpoint.Set(Mathf.Cos(deg* Mathf.Deg2Rad)* radius,0,Mathf.Sin(deg* Mathf.Deg2Rad)* radius);
                vertices.Add(newpoint);
                triangles.Add(0);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);

                deg += meshSize;
                lastpoint.Set(newpoint.x, newpoint.y, newpoint.z);
            }
            triangles.Add(0);
            triangles.Add(vertices.Count-1);
            triangles.Add(1);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }
        Gizmos.DrawMesh(mesh,center);
    }

    public static void DrawSectorMesh(Vector3 center,Vector3 dir,float radiusMin,float radiusMax,int deg,Quaternion quat,Mesh mesh)
    {
        if(mesh== null)
        {
            mesh = new Mesh();
            mesh.Clear();
            dir.y = 0;
            dir.Normalize();
            float val = Mathf.Asin(dir.z);
            float dirdeg = (Mathf.Asin(dir.z)+(dir.z>0 ? (dir.x< 0? 90:0):(dir.x< 0? -90: 0))) *Mathf.Rad2Deg;
            float halfdeg = Mathf.Abs(deg) * 0.5f;
            int startdeg = (int)(dirdeg-halfdeg);
            int enddeg = (int)(dirdeg+halfdeg);

            Vector3 lastPointMin = new Vector3(Mathf.Cos(startdeg* Mathf.Deg2Rad)* radiusMin,0,Mathf.Sin(startdeg* Mathf.Deg2Rad)* radiusMin);
            Vector3 lastPointMax = new Vector3(Mathf.Cos(startdeg * Mathf.Deg2Rad) * radiusMax, 0, Mathf.Sin(startdeg * Mathf.Deg2Rad) * radiusMax);
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            vertices.Add(lastPointMin);
            vertices.Add(lastPointMax);

            float drawdeg = startdeg;
            while (drawdeg <= enddeg)
            {
                drawdeg += meshSize;
                if (drawdeg > enddeg)
                    drawdeg = enddeg;

                lastPointMin = new Vector3(Mathf.Cos(drawdeg * Mathf.Deg2Rad) * radiusMin, 0, Mathf.Sin(drawdeg * Mathf.Deg2Rad) * radiusMin);
                lastPointMax = new Vector3(Mathf.Cos(drawdeg * Mathf.Deg2Rad) * radiusMax, 0, Mathf.Sin(drawdeg * Mathf.Deg2Rad) * radiusMax);
                vertices.Add(lastPointMin);
                vertices.Add(lastPointMax);

                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 4);


                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 3);

                if (drawdeg >= enddeg)
                    break;
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }
        Gizmos.DrawMesh(mesh,center,quat);
    }


    /// <summary>
    /// 绘制圆环
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radiusmin"></param>
    /// <param name="radiusmax"></param>
    public static void DrawRing(Vector3 center,float radiusmin,float radiusmax)
    {
        DrawCirCle(center,radiusmin);
        DrawCirCle(center, radiusmax);
    }

    public static void DrawRingMesh(Vector3 center,float radiusMin,float radiusMax,Mesh mesh)
    {
        if(mesh == null)
        {
            mesh = new Mesh();
            mesh.Clear();
            float deg = 0;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            Vector3 lastPointMin = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radiusMin, 0, Mathf.Sin(deg * Mathf.Deg2Rad) * radiusMin);
            Vector3 lastPointMax = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radiusMax, 0, Mathf.Sin(deg * Mathf.Deg2Rad) * radiusMax);
          
            vertices.Add(lastPointMin);
            vertices.Add(lastPointMax);

            while (deg <= 360)
            {
                deg += meshSize;

                lastPointMin = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radiusMin, 0, Mathf.Sin(deg * Mathf.Deg2Rad) * radiusMin);
                lastPointMax = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad) * radiusMax, 0, Mathf.Sin(deg * Mathf.Deg2Rad) * radiusMax);
                vertices.Add(lastPointMin);
                vertices.Add(lastPointMax);

                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 4);


                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 3);

            }

            triangles.Add(vertices.Count - 2);
            triangles.Add(1);
            triangles.Add(0);


            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
            triangles.Add(1);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }
        Gizmos.DrawMesh(mesh, center);
    }

    public static void DrawRectangleMesh(Vector3 center,Vector3 start,Vector3 end,float width,Quaternion quat,Mesh mesh)
    {
        if(mesh == null)
        {
            mesh = new Mesh();
            mesh.Clear();

            float halfWidth = width * 0.5f;
            Vector3 dir = end - start;
            Vector3 vdir = Vector3.Cross(dir,Vector3.up).normalized;
            Vector3 leftup = start + vdir * halfWidth;
            Vector3 leftdown = start - vdir * halfWidth;

            Vector3 rightup = end + vdir * halfWidth;
            Vector3 rightdown = end - vdir * halfWidth;

            List<Vector3> vertices = new List<Vector3>()
            {
                leftup,leftdown,rightup,rightdown
            };
            List<int> triangles = new List<int>()
            {
                1,0,2,1,2,3
            };

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }
        Gizmos.DrawMesh(mesh,center,quat);
    }

    public static void DrawParabola(Vector3 start, Vector3 speed, Vector3 acc, float time,int stepCount= 100)
    {
        Vector3 last = start;
        for(int i=0;i<stepCount; i++)
        {
            float cur = Mathf.Lerp(0, time, i / (float)stepCount);

            Vector3 pos = start + speed * cur + 0.5f * acc * cur * cur;
            Gizmos.DrawLine(last,pos);
            last = pos;
        }
    }
}
