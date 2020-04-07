using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class IContainerHelper
{
    public static V GetOrCreateValue<K,V>(this IDictionary<K,V> dic,K key) where V:new()
    {
        if (!dic.ContainsKey(key))
            dic.Add(key,new V());
        return dic[key];
    }

    public static V GetValueOrDefault<K,V>(this IDictionary<K,V> dic,K key,V defaultValue)
    {
        V ret = defaultValue;
        if (dic.TryGetValue(key, out ret))
            return ret;
        return defaultValue;
    }

    public static void AddOrUpdate<K,V>(this IDictionary<K,V> dic,K key,V value)
    {
        if (dic.ContainsKey(key))
            dic[key] = value;
        else
            dic.Add(key,value);
    }

    public static bool? GetAsBool<K>(this IDictionary<K,object> dic,K key)
    {
        return dic.GetValueOrDefault(key,null) as bool?;
    }

    public static int? GetAsInt<K>(this IDictionary<K, object> dic, K key)
    {
        return dic.GetValueOrDefault(key, null) as int?;
    }
    public static float? GetAsfloat<K>(this IDictionary<K, object> dic, K key)
    {
        return dic.GetValueOrDefault(key, null) as float?;
    }

    public static string GetAsString<K>(this IDictionary<K, object> dic, K key)
    {
        return dic.GetValueOrDefault(key, null) as string;
    }

    public static void Each<T>(this IEnumerable<T> container,Action<T,int> action)
    {
        int i = 0;
        foreach (T x in container)
            action(x, i++);
    }
    public static void Each<T>(this IEnumerable<T> container, Action<T> action)
    {
        foreach (T x in container)
            action(x);
    }

    public static bool IsCondition<T>(this IEnumerable<T> container,Func<T,bool> func)
    {
        foreach(T x in container)
        {
            if (true == func(x)) return true;
        }
        return false;
    }

    public static T RandomElement<T>(this IList<T> container)
    {
        return container[UnityEngine.Random.Range(0, container.Count)];
    }

    public static bool AddNotOverlap<T>(this IList<T> container,T item)
    {
        if(false == container.Contains(item)) { container.Add(item); return true; }
        return false;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> container)
    {
        return null == container || container.Count() < 1;
    }

}

