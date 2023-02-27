using System;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionData : ScriptableObject
{
    [Serializable]
    public struct Clip
    {
        public string name;
        public int index;
        public int frameCount;
        public List<Vector3> positions;
        public Vector3 destination;
    }

    [Serializable]
    public struct Data
    {
        public string name;
        public List<Clip> clips;
        public short fps;
    }

    [SerializeField] private List<Data> m_RootMotions = new List<Data>();

    public Data Get(string strName)
    {
        int nFindIndex = m_RootMotions.FindIndex(x => x.name == strName);
        if(nFindIndex== -1)
        {
            return new Data();
        }
        return m_RootMotions[nFindIndex];
    }

    public void Clear()
    {
        m_RootMotions.Clear();
    }

    public void Insert(string strKey,short fps,Dictionary<string,List<Vector3>> DicData)
    {
        for(int i=m_RootMotions.Count-1;i>=0;i--)
        {
            if(m_RootMotions[i].name.CompareTo(strKey) ==0)
            {
                m_RootMotions.RemoveAt(i);
            }
        }

        Data data;
        data.name = strKey;
        data.fps = fps;
        data.clips = new List<Clip>();
        int nIndex = 0;
        foreach(KeyValuePair<string,List<Vector3>> Pair in DicData)
        {
            Clip clip;
            clip.name = Pair.Key;
            clip.index = nIndex;
            clip.frameCount = Pair.Value.Count;

            if(Pair.Value.Count == 0)
            {
                clip.destination = Vector3.zero;
            }
            else
            {
                clip.destination = Pair.Value[Pair.Value.Count - 1];
            }

            clip.positions = new List<Vector3>();
            clip.positions.Add(Vector3.zero);
            for(int i=0;i<Pair.Value.Count;i++)
            {
                clip.positions.Add(Pair.Value[i]);
            }

            data.clips.Add(clip);
            nIndex++;
        }

        m_RootMotions.Add(data);
    }

    public void Remove(Data data)
    {
        if(!string.IsNullOrEmpty(data.name))
        {
            m_RootMotions.Remove(data);
        }
    }

    public void Append(Data data)
    {
        int nFindIndex = m_RootMotions.FindIndex(x=>x.name == data.name);
        if (nFindIndex == -1)
        {
            m_RootMotions.Add(data);
        }
        else
        {
            m_RootMotions[nFindIndex] = data;
        }
    }
}
