using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

namespace GameFrame.AssetBundles
{
    public class AssetBundlePath
    {
        /// <summary>
        /// 给WWW加载接口使用
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="isAssetBundlesFolder"></param>
        /// <returns></returns>
        public static string GetStreamingAssetsFilePath(string assetPath = null, bool isAssetBundlesFolder = true)
        {
            string outputPath = string.Empty;
            if (isAssetBundlesFolder)
            {
#if UNITY_ANDROID
                outputPath = Path.Combine(Application.streamingAssetsPath,AssetBundleConfig.AssetBundlesFolderName);
#else
                outputPath = Path.Combine("file://" + Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
#endif
            }
            else
            {
#if UNITY_ANDROID
                outputPath = Application.streamingAssetsPath;
#else
                outputPath = "file://" + Application.streamingAssetsPath;
#endif
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
        }


        public static string GetStreamingAssetsDataPath(string assetPath = null)
        {
            string outputPath = string.Empty;

            outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);

            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
            return outputPath;
        }

        public static string GetPersistentFilePath(string assetPath = null)
        {
            return "file://" + GetPersistentDataPath(assetPath);
        }

        public static string GetPersistentDataPath(string assetPath = null)
        {
            string outputPath = Path.Combine(Application.persistentDataPath,AssetBundleConfig.AssetBundlesFolderName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath,assetPath);
            }
#if UNITY_EDITOR
            return AssetBundleUtils.FormatToSysFilePath(outputPath);
#else
            return outputPath;
#endif
        }


        /// <summary>
        /// 注意：这个路径是读文件使用的Url,如果要直接磁盘写persistentDataPath，使用GetPlatformPersistentDataPath
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetAssetBundleFileUrl(string filePath)
        {
            if(CheckPersistentFileExsits(filePath))
            {
                return GetPersistentDataPath(filePath);
            }
            else
            {
                return GetStreamingAssetsDataPath(filePath);
            }
        }

        /// <summary>
        /// 注意：获取Bundle全路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetAssetBundleDataPath(string filePath)
        {
            if(CheckPersistentFileExsits(filePath))
            {
                return GetPersistentDataPath(filePath);
            }
            else
            {
                return GetStreamingAssetsDataPath(filePath);
            }
        }

        public static bool CheckPersistentFileExsits(string filePath)
        {
            var path = GetPersistentDataPath(filePath);
            return File.Exists(path);
        }

        public static string AssetBundlePathToAssetBundleName(string assetPath)
        {
            if(!string.IsNullOrEmpty(assetPath))
            {
                if (assetPath.StartsWith(AssetBundleConfig.AssetsFolderName + "/"))
                {
                    //移除"Assets/AssetsPackage"
                    assetPath = AssetsPathToPackagePath(assetPath);
                }

                // no""
                assetPath = assetPath.Replace(" ", "");

                //中文转成unicode
                assetPath = ConvertChineseToUnicode(assetPath);
                //把一些特殊符号改为下划线，例如"@"符号导致文件不能上传CDN
                assetPath = assetPath.Replace("@","_");
                assetPath = assetPath.Replace("#", "_");

                //there shou not be any '.' in the assetbundle name
                //otherwise the variant handling in client may go wrong
                assetPath = assetPath.Replace(".", "_");
                // add after suffix ".assetbundle" to the end
                assetPath = assetPath + AssetBundleConfig.AssetBundleSuffix;
                return assetPath.ToLower();
            }
            return null;
        }

        private const string chineseRegexStr = @"[\u4e00-\u9fa5]";

        private static string ConvertChineseToUnicode(string str)
        {
            string outstr = "";
            if(!string.IsNullOrEmpty(str))
            {
                for(int i=0,Imax=str.Length;i<Imax;i++)
                {
                    if(Regex.IsMatch(str[i].ToString(),chineseRegexStr))
                    {
                        //不需要
                        outstr += ((int)str[i]).ToString("x");
                    }
                    else
                    {
                        outstr += str[i];
                    }
                }
            }
            return outstr;
        }

        /// <summary>
        /// 拼接"Assets/AssetsPackage"
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string PackagePathToAssetsPath(string assetPath)
        {
            return AssetBundleConfig.AssetsFolderName + "/" + AssetBundleConfig.AssetsPackageName + "/" + assetPath;
        }

        /// <summary>
        /// 是否包含"Assets/AssetsPackge/"
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static bool IsPackagePath(string assetPath)
        {
            string path = AssetBundleConfig.AssetsFolderName + "/" + AssetBundleConfig.AssetsPackageName + "/";
            return assetPath.StartsWith(path);
        }

        public static string AssetsPathToPackagePath(string assetPath)
        {
            string path = AssetBundleConfig.AssetsFolderName + "/" + AssetBundleConfig.AssetsPackageName + "/";
            string path_Asset = AssetBundleConfig.AssetsFolderName + "/";
            if(assetPath.StartsWith(path))
            {
                return assetPath.Substring(path.Length);
            }
            else if(assetPath.StartsWith(path_Asset))
            {
                return assetPath.Substring(path_Asset.Length);
            }
            else
            {
                Debug.LogError("Asset path is not a packgae path!");
                return assetPath;
            }
        }

        public static string GetFullPath(string assetPath)
        {
            string result = CombinePath(Application.dataPath, PackagePathToAssetsPath(assetPath).Replace(AssetBundleConfig.AssetsFolderName + "/", string.Empty));
            return result;
        }

        public static string CombinePath(string path0,string path1)
        {
            string result = path0;
            if (result.Length > 0)
            {
                if (result.EndsWith("\\"))
                    result = result.Substring(0, result.Length - 1);
                if (!result.EndsWith("/"))
                    result += "/";
                result += path1;
            }
            else
                result = path1;
            return result;
        }
    }
}
