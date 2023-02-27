using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SkillEditorUtils
{
    public const string DIALOG_TITLE = "BigWorld";
    public static Texture2D m_AALineTex = null;
    public static Texture2D m_LineTex = null;
    static readonly Color splitterDark = new Color(0.12f,0.12f,0.12f,1.333f);
    static readonly Color splitterLight = new Color(0.6f,0.6f,0.6f,1.333f);

    public static Color splitter { get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; } }

    public static bool ShowDialog(string info)
    {
        return EditorUtility.DisplayDialog(DIALOG_TITLE,info,"OK");
    }

    public static bool ShowDialog(string info,string ok,string cancel)
    {
        return EditorUtility.DisplayDialog(DIALOG_TITLE,info,ok,cancel);
    }

    public static void ShowProgressBar(string info,int progress,int maxProgress)
    {
        info = string.Format(info+"({0}/{1})",progress,maxProgress);
        EditorUtility.DisplayProgressBar(DIALOG_TITLE,info,(float) progress/maxProgress);
    }

    public static bool DrawHeader(string caption,string key,bool defaultExpanded = true)
    {
        GUILayout.Space(3.0f);
        key = key + caption;
        bool expanded = EditorPrefs.GetBool(key,defaultExpanded);
        if (!expanded) GUI.backgroundColor = new Color(0.8f,0.8f,0.8f);

        EditorGUILayout.BeginHorizontal();
        GUI.changed = false;

        if (expanded) caption = "\u25BC" + (char)0x200a + caption;
        else caption = "\u25BA"+(char)0x200a+caption;

        GUILayout.BeginHorizontal();
        GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f,0f,0f,0.7f);
        if (!GUILayout.Toggle(true, caption, "dragtab", GUILayout.MinWidth(20f))) expanded = !expanded;
        GUI.contentColor = Color.white;
        if (GUI.changed) EditorPrefs.SetBool(key,expanded);
        GUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!expanded) GUILayout.Space(3.0f);

        return expanded;
    }

    private static int _beginCounter = 0;

    public static void BeginContents()
    {
        _beginCounter++;
        GUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal("TextArea",GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndContents()
    {
        if(_beginCounter>0)
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);

            _beginCounter--;
        }
    }

    /// <summary>
    /// Draws a horizontal split line
    /// </summary>
    public static void DrawSplitter()
    {
        var rect = GUILayoutUtility.GetRect(1f,1f);

        //Splitter rect should be full-width
        rect.xMin = 0f;
        rect.width += 4f;

        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect,splitter);
    }

    /// <summary>
    /// 获取某个目录下的指定的子目录
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static DirectoryInfo GetDirectoryInfoOrCreate(DirectoryInfo parent,string pattern)
    {
        DirectoryInfo[] resultArray = parent.GetDirectories(pattern);

        DirectoryInfo ret;
        if(resultArray.Length>0)
        {
            ret = resultArray[0];
        }
        else
        {
            ret = parent.CreateSubdirectory(pattern);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
        return ret;
    }

    public static string PathNormalized(string rawPath)
    {
        return rawPath.Replace(@"\\","/").Replace(@"\","/");
    }

    /// <summary>
    /// 拼合路径并格式化
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    public static string PathCombineNormalized(string path1,string path2)
    {
        string path = Path.Combine(path1,path2);
        return PathNormalized(path);
    }

    /// <summary>
    /// 将全路径格式化为Assets/...的路径，供Unity函数调用
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string FormatAssetsPath(string src)
    {
        int nindex = src.IndexOf("Assets");
        if (nindex < 0) return src;
        return src.Substring(nindex);
    }

    internal static void MoveAsset(string oldPath,string newPath)
    {
        try
        {
            AssetDatabase.MoveAsset(oldPath,newPath);
        }
        catch(Exception)
        {
            Debug.LogErrorFormat("Move Asset From {0} to {1} Failed",oldPath,newPath);
        }
        finally 
        {

        }
    }
    

    public static void DrawLine(Vector2 pointA,Vector2 pointB,Color color,float width,bool antiAlias)
    {
        Color saveColor = GUI.color;
        Matrix4x4 saveMatrix = GUI.matrix;

        if(!m_LineTex)
        {
            m_LineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
            m_LineTex.SetPixel(0,1,Color.white);
            m_LineTex.Apply();
        }

        if(!m_AALineTex)
        {
            m_AALineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
            m_AALineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
            m_AALineTex.SetPixel(0, 1, Color.white);
            m_AALineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
            m_AALineTex.Apply();
        }
        if (antiAlias) width *= 3;
        float angle = Vector3.Angle(pointB-pointA,Vector2.right) * (pointA.y <= pointB.y?1:-1);
        float m = (pointB - pointA).magnitude;
        if(m> 0.01f)
        {
            Vector3 dz = new Vector3(pointA.x,pointA.y,0);

            GUI.color = color;
            GUI.matrix = translationMatrix(dz) * GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(m,width),new Vector3(-0.5f,0,0));
            GUI.matrix = translationMatrix(-dz) * GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, Vector2.zero);
            GUI.matrix = translationMatrix(dz+new Vector3(width/2,-m/2)* Mathf.Sin(angle*Mathf.Deg2Rad))* GUI.matrix;

            if(!antiAlias)
            {
                GUI.DrawTexture(new Rect(0, 0, 1, 1), m_LineTex);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, 1, 1), m_AALineTex);
            }
        }
        GUI.matrix = saveMatrix;
        GUI.color = saveColor;
    }

    public static void DrawLineVH(Vector2 pointA,Vector2 pointB,Color color)
    {
        Color saveColor = GUI.color;

        if(!m_LineTex)
        {
            m_LineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
            m_LineTex.SetPixel(0,1,Color.white);
            m_LineTex.Apply();
        }

        if(!m_AALineTex)
        {
            m_AALineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
            m_AALineTex.SetPixel(0,0,new Color(1,1,1,0));
            m_AALineTex.SetPixel(0,1,Color.white);
            m_AALineTex.SetPixel(0,2,new Color(1,1,1,0));
            m_AALineTex.Apply();
        }

        GUI.color = color;
        if (pointB.y == pointA.y)
        {
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, pointB.x-pointA.x, 1), m_LineTex);
        }
        else
        {
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, pointB.y- pointA.y), m_LineTex);
        }

        GUI.color = saveColor;
    }

    public static void bezierLine(Vector2 start,Vector2 startTangent,Vector2 end,Vector2 endTangent,Color color,float width,bool antiAlias,int segments)
    {
        Vector2 lastV = cubeBezier(start,startTangent,end,endTangent,0);
        for(int i=1;i<= segments;++i)
        {
            Vector2 v = cubeBezier(start, startTangent, end, endTangent, i / (float)segments);
            DrawLine(lastV,v,color,width,antiAlias);
            lastV = v;
        }
    }

    private static Vector2 cubeBezier(Vector2 s,Vector2 st,Vector2 e,Vector2 et,float t)
    {
        float rt = 1 - t;
        float rtt = rt * t;
        return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
    }

    private static Matrix4x4 translationMatrix(Vector3 v)
    {
        return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
    }

    public static Transform FindChildByName(string strName,Transform trans)
    {
        if(trans.name == strName)
        {
            return trans;
        }

        Transform ret = null;
        for(int i=0;i<trans.childCount;i++)
        {
            ret = FindChildByName(strName, trans.GetChild(i));
            if(ret!= null)
            {
                return ret;
            }
        }
        return null;
    }
    
}
