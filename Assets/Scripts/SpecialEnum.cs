using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Flags]
public enum BigTypes 
{
    Type1,
    Type2,
    Type3,
    Type4,
}
public class SpecialEnum : MonoBehaviour
{
    [EnumFlags]
    public BigTypes bigType;


    public bool IsSelectEnumType(BigTypes type)
    {
        //将枚举值转换为int类型，1 左移
        int index = 1 << (int)type;

        int eventTypeResult = (int)bigType;

        if((eventTypeResult & index)== index)
        {
            return true;
        }
        return false;

    }
   
}
