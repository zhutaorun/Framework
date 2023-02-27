using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillConfig
{
    namespace skill_config
    {
       [System.Serializable]
       public partial class FrameEvent
       {
            public int EventTimePoint;
            public int FrameEventId;
           
            
       }
        [System.Serializable]
        public partial class SkillActionInfo
        {
            public int ActionTime;
            public int ActionStartPoint;
        }
        [System.Serializable]
        public partial class skill_data
        {
            public int Id;
        }
        [System.Serializable]
        public partial class effect_data
        {
            public string Path;
        }
    }


    public class ConfigContainer<TKey,TValue>
    {
        [SerializeField]
        public List<TKey> keys = new List<TKey>();

        [SerializeField]
        public List<TValue> values = new List<TValue>();

        public Dictionary<TKey, object> dict = new Dictionary<TKey, object>();


        public void Add(TKey key,TValue value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public void Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        public Dictionary<TKey, TValue> ToDict()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                TKey key = keys[i];
                TValue value = values[i];
                if (!dict.ContainsKey(key))
                {
                    dict.Add(keys[i], value);
                }
                else
                {
                    throw new Exception("重复的key:" + key.ToString());
                }
            }
            return dict;
        }

        public Dictionary<TKey,object> ToObjectDict()
        {
            Dictionary<TKey, object> dict = new Dictionary<TKey, object>();
            for(int i=0;i<keys.Count;i++)
            {
                TKey key = keys[i];
                TValue value = values[i];
                if(!dict.ContainsKey(key))
                {
                    dict.Add(keys[i],value);
                }
                else
                {
                    throw new Exception("重复的key:"+key.ToString());
                }
            }
            return dict;
        }
    }
}
