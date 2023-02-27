using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFrame;
using GameFrame.Utils;
using System;
using System.IO;

public class Description : SingletonNew<Description>,IDescription
{
    private Dictionary<string, Dictionary<string, string>> Data = new Dictionary<string, Dictionary<string, string>>();
    private bool HaveDone = true;
    private string m_Path = Application.dataPath + "/Utils/Network/RPC/Rsb/Protocols/ProtocolTable.cs";

    public override void Init()
    {
        base.Init();
        LoadFile();
    }

    public void Clear()
    {
        HaveDone = true;
        Data.Clear();
    }

    public string GetDescription<T>(string name)
    {
        return GetDescription(typeof(T).Name,name);
    }

    public string GetDescription(string name,Type type)
    {
        return GetDescription(type.Name,name);
    }

    public string GetDescription(string typeName,string fieldName)
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

    public void LoadFolder(string folder = "",bool includeChildFolder = true)
    {

    }

    public void LoadFile(string path ="",bool clear = true)
    {
        if (path != string.Empty)
            m_Path = path;
        if (clear)
            Clear();
        if (!HaveDone)
            return;
        Analysis();
    }

    private void Analysis()
    {
        HaveDone = false;

        string path = m_Path;
        FileStream fs = null;
        StreamReader streamReader = null;
        try
        {
            if(!File.Exists(path))
            {
                return;
            }
            fs = File.Open(path,FileMode.Open);
            streamReader = new StreamReader(fs);
            while(!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string name = string.Empty;
                if(line.Contains(" class "))
                {
                    name = GetClassName(line);

                    Dictionary<string, string> result = ReadClass(streamReader,';');
                    if (!Data.ContainsKey(name))
                        Data.Add(name, result);
                    else
                        Data[name] = result;
                }
                else if(line.Contains(" enum "))
                {
                    name = GetEnumName(line);
                    Dictionary<string, string> result = ReadClass(streamReader,',');
                    if (!Data.ContainsKey(name))
                        Data.Add(name, result);
                    else
                        Data[name] = result;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        finally
        {
            if (streamReader != null)
                streamReader.Dispose();
            if (fs != null)
                fs.Dispose();
        }
        HaveDone = true;
    }

    private Dictionary<string,string> ReadClass(StreamReader streamReader,char symbol)
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

            if(line.Trim() == "{")//过滤
            {
                index++;
                continue;
            }
            else if(line.Trim() =="}")//过滤
            {
                index--;
            }
            if (index <= 0)
                break;
            else if (index > 1)
                continue;

            //过滤非语句非枚举行(包含；或，)
            if(!line.Contains(symbol.ToString()))
            {
                continue;
            }

            string name = string.Empty;
            string des = string.Empty;
            AnalysisProperty(line, ref name, ref des, symbol);
            if(name != string.Empty && des != string.Empty)
            {
                if (!result.ContainsKey(name))
                    result.Add(name, des);
                else
                    Debug.LogErrorFormat("the same key is already exist,why? key:{0},line:{1}",name,line);
            }
        }
        return result;
    }

    private void AnalysisProperty(string line,ref string name,ref string des,char symbo)
    {
        if (line == null || line == string.Empty) return;
        if(line.Contains("Dictionary<int,SAttrValue> AttrEffectDic"))
        {
            Debug.Log("");
        }

        string[] strs = line.Split('/');
        if (strs.Length > 1)
            des = strs[strs.Length - 1];
        strs = line.Split(symbo);
        if (strs == null || strs.Length <= 0) return;

        string dest = strs[0];
        if(dest.Contains("="))
        {
            dest = dest.Split('=')[0];
        }
        else if(dest.Contains("{"))
        {
            dest = dest.Substring(0,dest.IndexOf("{"));
        }
        if (symbo == ',')
            name = dest.Trim();
        else
        {
            dest = dest.Trim();
            strs = dest.Split(' ');
            name = strs[strs.Length - 1];
        }
    }

    private string GetClassName(string line)
    {
        string[] strs = line.Split(' ');
        for (int i = 0; i < strs.Length; i++) 
        {
            if(strs[i].Trim() == "class" && i<strs.Length-1)
            {
                return strs[i + 1].Trim();
            }
        }
        return string.Empty;
    }

    private string GetEnumName(string line)
    {
        if(line.Contains(";"))
        {
            line = line.Split(';')[0];
        }
        string[] strs = line.Split(' ');
        for(int i= 0;i<strs.Length;i++)
        {
            if(strs[i].Trim() == "enum" && i< strs.Length-1)
            {
                return strs[i + 1].Trim();
            }
        }
        return string.Empty;
    }

    public static bool TryGetDesType(string des,List<Type> typeList)
    {
        if (typeList == null) return false;

        typeList.Clear();
        if(des.IsNullOrEmpty())
        {
            return false;
        }

        bool record = false;
        string value = "";
        for(int i=0;i<des.Length;i++)
        {
            char c = des[i];
            if(c== '@')
            {
                record = !record;
                //记录结束
                if(!record)
                {
                    Type type = Type.GetType(value);
                    if(type!=null)
                    {
                        typeList.Add(type);
                    }
                    value = "";
                }
                continue;
            }
            if(record)
            {
                value += c;
            }
        }
        return typeList.Count > 0;
    }
    public string[] GetEnumDesArray(string name)
    {
        throw new NotImplementedException();
    }
}
