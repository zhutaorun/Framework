using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class Debug 
{
    static private bool bShowLog = true;

    static private Color normalColor = Color.green;
    static private Color warningColor = new Color(1.0f,0.5f,0.15f);
    static private Color errorColor = Color.red;

    struct LogInfo
    {
        public string des;
        public float start_time;
    }

    static private Dictionary<string, LogInfo> m_log_info_map = new Dictionary<string, LogInfo>();

    public static bool IsShowLog
    {
        get { return bShowLog; }
        set { bShowLog = value; }
    }

    static Debug()
    {
#if !UNITY_EDITOR
        if(UnityEngine.Debug.unityLogger != null)
        {
            if(!UnityEngine.Debug.isDebugBuild)
            {
                UnityEngine.Debug.unityLogger.filterLogType = LogType.Exception;
            }
        }
#endif
    }

    static public void Release()
    {
        m_log_info_map.Clear();
    }

    public static bool developConsoleVisible
    {
        get { return UnityEngine.Debug.developerConsoleVisible; }
        set { UnityEngine.Debug.developerConsoleVisible = value; }
    }

    public static bool IsEnabled { get { return true; } }

    public static void Break() { if (IsEnabled) UnityEngine.Debug.Break(); }

    public static void DebugBreak() { if (IsEnabled) UnityEngine.Debug.DebugBreak(); }

    public static void ClearDeveloperConsole() { if (IsEnabled) UnityEngine.Debug.ClearDeveloperConsole(); }

    private static string ColorStartTag(Color color)
    {
#if UNITY_EDITOR
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}FF>",(int)(255f* color.r),(int)(255f* color.g),(int)(255f * color.b));
#else
        return "";
#endif
    }

    private static string ColorEndTag()
    {
#if UNITY_EDITOR
        return "</color>";
#else
        return "";
#endif
    }

    private static string ColorDecoratedString(Color color,string text)
    {
        return string.Format("{0}{1}{2}",ColorStartTag(color),text,ColorEndTag());
    }

    [Conditional("DEBUG_LOG")]
    public static void Log(object message)
    {
        if(bShowLog == true)
        {
            UnityEngine.Debug.Log(ColorDecoratedString(normalColor,message.ToString()));
        }
    }


    [Conditional("DEBUG_LOG")]
    public static void Log(object message,UnityEngine.Object context)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Log(ColorDecoratedString(normalColor, message.ToString()),context);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void Log(Color color, object message)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Log(ColorDecoratedString(color, message.ToString()));
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogFormat(string format,params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogFormat(ColorDecoratedString(normalColor,format),args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogFormat(context,ColorDecoratedString(normalColor, format), args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogWarning(object message)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogWarning(ColorDecoratedString(warningColor, message.ToString()));
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogWarning(object message,UnityEngine.Object context)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogWarning(ColorDecoratedString(warningColor, message.ToString()), context);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogWarningFormat(string format, params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogWarningFormat(ColorDecoratedString(warningColor, format), args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogWarningFormat(context, ColorDecoratedString(warningColor, format), args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogFormat(Color color,string format,params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogFormat(ColorStartTag(color)+format+ ColorEndTag(),args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogWarningFormat(Color color, string format, params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.LogWarningFormat(ColorStartTag(color) + format + ColorEndTag(), args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition)
    {
        if(bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition);
        }
    }


    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition,string message)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition, object message)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition, UnityEngine.Object context)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition, context);
        }
    }


    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition,string message, UnityEngine.Object context)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition, message,context);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void Assert(bool condition, object message, UnityEngine.Object context)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.Assert(condition, message, context);
        }

    }

    [Conditional("DEBUG_LOG")]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        if (bShowLog == true)
        {
            UnityEngine.Debug.AssertFormat(condition, format, args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogRecordTime(string key,bool is_over = false)
    {
        LogInfo info;
        if(m_log_info_map.TryGetValue(key,out info))
        {
            if(is_over)
            {
                StringBuilder builder = GameFrame.SharedStringBuilder.Instance;
                builder.Append("[结束记录]:");
                builder.Append(key);
                builder.Append("\n");
                builder.Append("消耗时间:");
                builder.Append(Time.realtimeSinceStartup - info.start_time);
                builder.Append("秒");
                Debug.LogWarning(builder.ToString());

                m_log_info_map.Remove(key);
            }
        }
        else
        {
            info = new LogInfo();
            info.start_time = Time.realtimeSinceStartup;
            info.des = key;
            m_log_info_map.Add(key,info);
            StringBuilder builder = GameFrame.SharedStringBuilder.Instance;
            builder.Append("[开始记录]:");
            builder.Append(key);
            Debug.LogWarning(builder.ToString());
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void DrawLine(Vector3 start,Vector3 end)
    {
        UnityEngine.Debug.DrawLine(start,end);
    }

    [Conditional("DEBUG_LOG")]
    public static void DrawLine(Vector3 start, Vector3 end,Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }

    [Conditional("DEBUG_LOG")]
    public static void LogError(object message)
    {
        if(bShowLog)
        {
            UnityEngine.Debug.LogError(ColorDecoratedString(errorColor,message.ToString()));
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogError(object message,UnityEngine.Object context)
    {
        if (bShowLog)
        {
            UnityEngine.Debug.LogError(ColorDecoratedString(errorColor, message.ToString()), context);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogErrorFormat(string format,params object[] args)
    {
        if (bShowLog)
        {
            UnityEngine.Debug.LogErrorFormat(ColorDecoratedString(errorColor, format), args);
        }
    }

    [Conditional("DEBUG_LOG")]
    public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
    {
        if (bShowLog)
        {
            UnityEngine.Debug.LogErrorFormat(context,ColorDecoratedString(errorColor, format), args);
        }
    }

    #region for release
    public static void LogException(Exception exception)
    {
        if(bShowLog)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
    public static void LogException(Exception exception,UnityEngine.Object context)
    {
        if (bShowLog)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
    }

    public static void LogException(string message)
    {
        if (bShowLog)
        {
            UnityEngine.Debug.LogException(new Exception(message));
        }
    }
    #endregion
}
