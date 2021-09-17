using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public enum Layer
    {
        Default = 0,
        TransparentFx= 1,
        IgnoreRaycast = 2,//引擎中的名称：Ignore Raycast
        Water = 4,
        UI = 5,
        Player=  8,
        Monster = 9,
        PostProcess = 12,
        UIRenderToTexture = 13,
        HideObject = 20,
        Max = 32,
    }

    public class UnityUtil
    {
       
    }

}