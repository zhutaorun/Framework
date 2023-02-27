using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace GameFrame
{
    public static class DeepCopyHelper
    {
       public enum ETargetType
        {
            Serialized,
            ProtobBuffer,
        }

        /// <summary>
        /// 深度复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T source,ETargetType targetType = ETargetType.ProtobBuffer) where T: class,new()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            switch(targetType)
            {
                case ETargetType.ProtobBuffer:
                    return (source as Google.Protobuf.IMessage).Clone() as T;
                case ETargetType.Serialized:
                    bf.Serialize(ms, source);
                    ms.Seek(0, 0);
                    T target = bf.Deserialize(ms) as T;
                    return target;
            }
            Debug.LogError("复制失败，未处理的目标类型:"+targetType);
            return null;
        }
    }
}
