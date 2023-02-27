using Google.Protobuf;
using System;
using System.IO;

namespace GameFrame
{
    /// <summary>
    /// PB扩展
    /// </summary>
    public static class ProtocolBufferExt
    {
        private static MemoryStream s_codeInputStream;
        private static BinaryWriter s_cacheWriter;

        /// <summary>
        /// Clone 一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static T Clone<T>(this T msg) where T:class,IMessage
        {
            if (msg == null)
                return null;

            Type type = msg.GetType();

            if(s_codeInputStream == null)
            {
                s_codeInputStream = new MemoryStream(ushort.MaxValue);
                s_cacheWriter = new BinaryWriter(s_codeInputStream);
            }
            s_cacheWriter.BaseStream.Seek(0,SeekOrigin.Begin);
            s_cacheWriter.BaseStream.SetLength(0);
            msg.WriteTo(s_cacheWriter.BaseStream);

            s_cacheWriter.BaseStream.Seek(0,SeekOrigin.Begin);
            IMessage newMessage = Activator.CreateInstance(type) as IMessage;
            newMessage.MergeFrom(s_cacheWriter.BaseStream);
            return newMessage as T;
        }


        /// <summary>
        /// 回收一个对象
        /// </summary>
        /// <param name="msg"></param>
        public static void Recycle(this IMessage msg)
        {
            if(msg == null)
            {
                return;
            }
            //ProtocolFactory.Instance.RecycleIMessage(msg);
        }

        public static string SaveToJson(this IMessage rsb)
        {
            return "";// ProtocolBufferJsonHelper.SaveToJson(rsb, true);
        }
    }
}
