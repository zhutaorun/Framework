using System.IO;
using System;
using System.Linq;
using UnityEngine;

namespace GameFrame.AssetBundles
{
    public class AssetBundleUtils
    {

        #region File
        public static string FormatToUnityPath(string path)
        {
            return path.Replace("\\","/");
        }

        public static string FormatToSysFilePath(string path)
        {
            return path.Replace("/","\\");
        }

        public static string FullPathToAssetPath(string full_path)
        {
            full_path = FormatToUnityPath(full_path);
            if (!full_path.StartsWith(Application.dataPath))
            {
                return null;
            }
            string ret_path = full_path.Replace(Application.dataPath,"");
            return AssetBundleConfig.AssetsFolderName + ret_path;
        }


        public static string GetFileExtension(string path)
        {
            return Path.GetExtension(path).ToLower();
        }

        public static string[] GetSpecifyFilesInFolder(string path, string[] extensions = null, bool exclude = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (extensions == null)
            {
                return Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);
            }
            else if(exclude)
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => !extensions.Contains(GetFileExtension(f))).ToArray();
            }
            else
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(GetFileExtension(f))).ToArray();
            }
        }


        public static string[] GetSpecifyFilesInFolder(string path,string patten)
        {
            if(string.IsNullOrEmpty(path))
            {
                return null;
            }
            return Directory.GetFiles(path,patten,SearchOption.AllDirectories);
        }

        public static string[] GetAllFilesInFolder(string path)
        {
            return GetSpecifyFilesInFolder(path);
        }

        public static string[] GetAllDirsInFolder(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        }


        public static void CheckFileAndCreateDirWhenNeeded(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FileInfo file_info = new FileInfo(filePath);
            DirectoryInfo dir_info = file_info.Directory;
            if(!dir_info.Exists)
            {
                Directory.CreateDirectory(dir_info.FullName);
            }
        }

        public static void CheckDirAndCreateWhenNeeded(string folderPath)
        {
            if(string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static bool SafeWriteAllBytes(string outFile,byte[] outBytes)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }
                CheckFileAndCreateDirWhenNeeded(outFile);
                if(File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllBytes(outFile,outBytes);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllBytes failed! path = {0} with err ={1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllLines(string outFile, byte[] outLines)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }
                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllBytes(outFile, outLines);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllLines failed! path = {0} with err ={1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllText(string outFile,string text)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }
                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllText(outFile, text);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllText failed! path = {0} with err ={1}", outFile, ex.Message));
                return false;
            }
        }

        public static byte[] SafeReadAllBytes(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }
                if (!File.Exists(inFile))
                {
                    return null;
                }
                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllBytes(inFile);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllBytes failed! path = {0} with err ={1}", inFile, ex.Message));
                return null;
            }
        }

        public static string[] SafeReadAllLines(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }
                if (!File.Exists(inFile))
                {
                    return null;
                }
                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllLines(inFile);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllLines failed! path = {0} with err ={1}", inFile, ex.Message));
                return null;
            }
        }

        public static string SafeReadAllText(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }
                if (!File.Exists(inFile))
                {
                    return null;
                }
                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllText(inFile);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllText failed! path = {0} with err ={1}", inFile, ex.Message));
                return null;
            }
        }


        public static void DeleteDirectory(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            Directory.Delete(dirPath, false);
                
        }

        public static bool SafeClearDir(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    return true;
                }
                if (Directory.Exists(folderPath))
                {
                    DeleteDirectory(folderPath);
                }
                Directory.CreateDirectory(folderPath);
                return true;

            }
            catch(Exception ex)
            {
                Debug.LogError(string.Format("SafeClearDir failed! path = {0} with err ={1}", folderPath, ex.Message));
                return false;
            }
        }


        public static bool SafeDeleteDir(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    return true;
                }
                if (Directory.Exists(folderPath))
                {
                    DeleteDirectory(folderPath);
                }
                Directory.CreateDirectory(folderPath);
                return true;

            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteDir failed! path = {0} with err ={1}", folderPath, ex.Message));
                return false;
            }
        }

        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }
                if (!File.Exists(filePath))
                {
                    return true;
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;

            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err ={1}", filePath, ex.Message));
                return false;
            }
        }


        public static bool SafeRenameFile(string sourecFileName,string destFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(sourecFileName))
                {
                    return false;
                }
                if (!File.Exists(sourecFileName))
                {
                    return true;
                }
                SafeDeleteFile(destFileName);
                File.SetAttributes(sourecFileName, FileAttributes.Normal);
                File.Move(sourecFileName, destFileName);
                return true;

            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeRenameFile failed! path = {0} with err ={1}", sourecFileName, ex.Message));
                return false;
            }
        }


        public static bool SafeCopyFile(string fromFile, string toFile)
        {
            try
            {
                if (string.IsNullOrEmpty(fromFile))
                {
                    return false;
                }
                if (!File.Exists(fromFile))
                {
                    return false;
                }
                CheckFileAndCreateDirWhenNeeded(toFile);
                SafeDeleteFile(toFile);
                File.Copy(fromFile, toFile,true);
                return true;

            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("SafeCopyFile failed! fromfile = {0},tofile = {1}, with err ={2}", fromFile,toFile, ex.Message));
                return false;
            }
        }


        #endregion
    }
}
