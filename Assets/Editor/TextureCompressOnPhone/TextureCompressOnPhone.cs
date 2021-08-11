using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;

public class TextureCompressOnPhone
{
    private const string AndroidPlatformName = "Android";
    private const string IOSPlatformName = "iPhone";

    [MenuItem("Tools/Android/CompressTexture")]
    private static void CompressTextureOnAndroid()
    {
        CompressInPlatform(AndroidPlatformName);
    }

    [MenuItem("Tools/IOS/CompressTexture")]
    private static void CompressTextureOnIOS()
    {
        CompressInPlatform(IOSPlatformName);
    }

    private static void CompressInPlatform(string platform)
    {
        var messgae = "You are going to perform the following operation to ALL Texture2D! continue?\n";

        //弹出对话框进行二次确认
        if (EditorUtility.DisplayDialog("Change Max Size", messgae, "OK", "Cancel") == false)
        {
            return;
        }

        //显示进度条
        EditorUtility.DisplayProgressBar("Change Max Size", "", 0.0f);

        //找到所有纹理
        var guids = AssetDatabase.FindAssets("t:Texture2D");

        for (int i = 0; i < guids.Length; i++)
        {
            var dialogTitel = "Change Max Size" + "(" + i + "/" + guids.Length + ")";
            var progress = (float)i / guids.Length;
            var guid = guids[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (go == null)
            {
                continue;
            }

            EditorUtility.DisplayProgressBar(dialogTitel, go.name, progress);

            //拿到TextureImporter才能对图片进行压缩
            var assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter != null)
            {
                var textureImporter = assetImporter as TextureImporter;
                if (textureImporter == null)
                {
                    Debug.LogErrorFormat(go, "MissingImporter Texture2D:'{0}'", go.name);
                    continue;
                }

                //拿到不压缩前的size
                var defaultMaxSize = textureImporter.maxTextureSize;
                TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings(platform);
                platformSettings.overridden = true;
                platformSettings.maxTextureSize = defaultMaxSize / 4;
                var width = 0;
                var height = 0;
                //拿到宽高
                GetTextureRealWidthAndHeight(textureImporter, ref width, ref height);

                //判断有无alpha
                var haveAlpha = textureImporter.DoesSourceTextureHaveAlpha();
                platformSettings.format = GetCompressFormat(platform, width, height, haveAlpha);

                textureImporter.SetPlatformTextureSettings(platformSettings);

                EditorUtility.SetDirty(go);
                Debug.LogFormat(go, "Processed Texture2D:'{0}'", go.name);
                textureImporter.SaveAndReimport();
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 得到目标压缩格式
    /// </summary>
    /// <returns></returns>
    private static TextureImporterFormat GetCompressFormat(string platform, int width, int height, bool haveAlpha)
    {
        //分几种情况处理，是不是2的次幂以及是不是有alpha通道
        var isPowerOfTwo = WidthAndHeightIsPowerOfTwo(width, height);
        if (isPowerOfTwo == false)
        {
            if (haveAlpha)
            {
                return TextureImporterFormat.RGBA16;
            }
            else
            {
                return TextureImporterFormat.RGB16;
            }
        }
        else
        {
            if (platform == AndroidPlatformName)
            {
                if (haveAlpha)
                {
                    return TextureImporterFormat.ETC2_RGBA8Crunched;
                }
                else
                {
                    return TextureImporterFormat.ETC_RGB4Crunched;
                }
            }
            else if (platform == IOSPlatformName)
            {
                if (haveAlpha)
                {
                    return TextureImporterFormat.PVRTC_RGBA4;
                }
                else
                {
                    return TextureImporterFormat.PVRTC_RGB4;
                }
            }
        }
        return TextureImporterFormat.RGBA16;
    }

    /// <summary>
    /// 判断宽和高是否2的次幂
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private static bool WidthAndHeightIsPowerOfTwo(int width, int height)
    { 
        if (IsPowerOfTwo(width)&& IsPowerOfTwo(height))
            return true;
        return false;
    }

    /// <summary>
    /// 用二进制与位算，如果一个数是2的次幂，n&(n-1)=0
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private static bool IsPowerOfTwo(int number)
    {
        if (number <= 0)
            return false;
        return (number & (number - 1)) == 0;
    }

    /// <summary>
    /// 得到要压缩成几分之一，这里是二分之一
    /// </summary>
    /// <param name="sourceMaxSize"></param>
    /// <returns></returns>
    private static int GetCompressMaxSize(int sourceMaxSize)
    {
        return sourceMaxSize / 2;
    }

    /// <summary>
    /// 用反射得到纹理宽高
    /// </summary>
    /// <param name="textureImporter"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void GetTextureRealWidthAndHeight(TextureImporter textureImporter,ref int width,ref int height)
    {
        System.Type type = typeof(TextureImporter);
        System.Reflection.MethodInfo method = type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);
        var args = new System.Object[] { width, height };
        method.Invoke(textureImporter,args);
        width = (int)args[0];
        height = (int)args[1];
    }
}

