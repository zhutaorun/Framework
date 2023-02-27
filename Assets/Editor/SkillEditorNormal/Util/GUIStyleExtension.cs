using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUIStyleExtension
{
    public static Color defaultButtonSelectColor = Color.cyan;

    public static readonly GUIStyle buttonStyleMid;
    public static readonly GUIStyle wideButtonStyle;

    private static Color lastBackGroundColor;
    private static Color lastContentColor;
    private static Color lastColor;


    static GUIStyleExtension()
    {
        buttonStyleMid = new GUIStyle(GUI.skin.button);

        wideButtonStyle = new GUIStyle(GUI.skin.button);
        wideButtonStyle.fontSize = 30;
        wideButtonStyle.fixedHeight = 40;
    }

    /// <summary>
    /// 修改并保存当前GUI参数
    /// </summary>
    /// <param name="backgroundColor">Global tinting color for all backround elements rendered by the GUI.</param>
    /// <param name="contentColor">Global tinting color for the GUI.</param>
    /// <param name="color">Tinting color for all text rendered by the GUI.</param>
    public static void ApplySkinStyle(Color backgroundColor,Color contentColor,Color color)
    {
        lastBackGroundColor = GUI.backgroundColor;
        lastContentColor = GUI.contentColor;
        lastColor = GUI.color;

        GUI.backgroundColor = backgroundColor == default(Color) ? GUI.backgroundColor : backgroundColor;
        GUI.contentColor = contentColor == default(Color) ? GUI.contentColor : contentColor;
        GUI.color = color == default(Color) ? GUI.color : color;
    }

    public static void RecoverSkinStyle()
    {
        GUI.backgroundColor = lastBackGroundColor;
        GUI.contentColor = lastContentColor;
        GUI.color = lastColor;
    }
}
