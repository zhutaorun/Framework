using System;

namespace GameFrame.Utils
{
    public interface IDescription 
    {
        void Clear();

        string GetDescription<T>(string name);

        string GetDescription(string name, Type type);

        string GetDescription(string typeName, string fieldName);

        string[] GetEnumDesArray(string name);
        void LoadFile(string path="",bool clear = true);

        void LoadFolder(string folder = "", bool includeChildFolder = true);
    }
}
