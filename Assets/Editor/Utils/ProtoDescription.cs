using GameFrame;
using GameFrame.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProtoDescription : SingletonNew<ProtoDescription>, IDescription
{
    enum EMsgType
    {
        Message,
        Enum,
    }

    private string m_Folder = UnityEngine.Application.dataPath + "/../../proto";//Proto文件夹
    private string m_Path;

    private Dictionary<string, Dictionary<string, string>> Data = new Dictionary<string, Dictionary<string, string>>();

    private Dictionary<string, string[]> EnumCache = new Dictionary<string, string[]>();

    public override void Init()
    {
        base.Init();
    }

    public override void UnInit()
    {
        base.UnInit();
    }
    public void Clear()
    {
        Data.Clear();
        EnumCache.Clear();
    }

    public string GetDescription<T>(string name)
    {
        return GetDescription(typeof(T).Name, name);
    }

    public string GetDescription(string name, Type type)
    {
        return GetDescription(type.Name, name);
    }

    public string GetDescription(string typeName, string fieldName)
    {
        Dictionary<string, string> dict = null;
        if(Data.TryGetValue(typeName,out dict))
        {
            string result = string.Empty;
            if (dict.TryGetValue(fieldName, out result))
                return result;
        }

        return fieldName;
    }


    public void LoadFolder(string folder = "", bool includeChildFolder = true)
    {
        if (folder != string.Empty)
            m_Folder = folder;
        if (!Directory.Exists(m_Folder))
        {
            Debug.LogError("目录不存在");
            return;
        }
        string fullPath = Path.GetFullPath(m_Folder);
        string[] files = includeChildFolder ?
            Directory.GetFiles(fullPath, "*.proto", SearchOption.AllDirectories) :
            Directory.GetFiles(fullPath, "*.proto", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            LoadFile(file, false);
        }
    }
    public void LoadFile(string path = "", bool clear = true)
    {
        if (path != string.Empty)
            m_Path = path;
        if(clear)
        {
            Clear();
        }
        FileStream fs = null;
        StreamReader streamReader = null;
        try
        {
            fs = File.Open(path,FileMode.Open);
            streamReader = new StreamReader(fs);
            Analysis(streamReader);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if(streamReader !=null)
            {
                streamReader.Dispose();
            }
            if(fs != null)
            {
                fs.Dispose();
            }
        }
    }

    private void Analysis(StreamReader streamReader)
    {
        while(!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            line = line.Trim();
            string name = string.Empty;
            if(line.StartsWith("message"))
            {
                name = GetMessageName(line);

                Dictionary<string, string> result = ReadMessage(streamReader);
                if (!Data.ContainsKey(name))
                    Data.Add(name, result);
                else
                    Data[name] = result;
            }
            else if(line.StartsWith("enum"))
            {
                name = GetMessageName(line);
                string[] result = ReadEnum(streamReader);
                if (!EnumCache.ContainsKey(name))
                    EnumCache.Add(name, result);
                else
                    EnumCache[name] = result;
            }
        }
    }

    private string GetMessageName(string line)
    {
        string[] strs = line.Split(' ');
        string name = strs[1].Trim();
        if(name.EndsWith("{"))
        {
            name = name.Substring(0,name.Length-1);
        }
        return name;
    }

    Dictionary<string,string> ReadMessage(StreamReader streamReader)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        int index = 0;
        while(!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine().Trim();
            //过滤注释
            if(line.StartsWith("//"))
            {
                continue;
            }

            if(line.Trim()=="{")//过滤
            {
                index++;
                continue;
            }
            else if (line.Trim() == "}")//过滤
            {
                index--;
                continue;
            }
            if (index <= 0)
                break;
            else if (index > 1)
                continue;

            //过滤非语句非枚举举行(包含;)
            if (!line.Contains(";"))
            {
                continue;
            }

            string name = string.Empty;
            string des = string.Empty;
            AnalysisProperty(line,ref name,ref des);
            if(name != string.Empty && des != string.Empty)
            {
                if (!result.ContainsKey(name))
                    result.Add(name, des);
                else
                    Debug.LogErrorFormat("the same key is already exist,why? key{0},line:{1}",name,line);
            }

        }
        return result;
    }

    private void AnalysisProperty(string line,ref string name,ref string des)
    {
        if (line == null || line == string.Empty) return;
        string[] strs = line.Split('/');
        if (strs.Length > 1)
            des = strs[strs.Length - 1];
        strs = line.Split(';');
        if (strs == null || strs.Length <= 0) return;

        string dest = strs[0];
        if(dest.Contains("="))
        {
            dest = dest.Split('=')[0];
        }

        dest = dest.Trim();
        strs = dest.Split(' ');
        name = strs[strs.Length - 1];
    }


    string[] ReadEnum(StreamReader streamReader)
    {
        List<string> result = new List<string>();
        int index = 0;
        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine().Trim();
            //过滤注释
            if (line.StartsWith("//"))
            {
                continue;
            }

            if (line.Trim() == "{")//过滤
            {
                index++;
                continue;
            }
            else if (line.Trim() == "}")//过滤
            {
                index--;
                continue;
            }
            if (index <= 0)
                break;
            else if (index > 1)
                continue;

            //过滤非语句非枚举 行(包含;)
            if (!line.Contains(";"))
            {
                continue;
            }

            string name = string.Empty;
            string des = string.Empty;
            AnalysisProperty(line, ref name, ref des);
            if (des != string.Empty)
            {
                result.Add(des);
            }
            else
            {
                result.Add(name);
            }
        }
        return result.ToArray();
    }
    public string[] GetEnumDesArray(string name)
    {
        if (!EnumCache.ContainsKey(name)) return null;
        return EnumCache[name];
    }
}
