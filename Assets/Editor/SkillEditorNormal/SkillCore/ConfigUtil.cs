using SkillnewConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.SkillDisplay
{

    public static class ConfigUtil
    {
        private const float TIME_TO_SECOND = 0.001f;
        private const int TIME_TO_MILLISECOND = 1000;
        private const float CM_TO_M = 0.01f;

        /// <summary>
        /// 转换时间毫秒为单位秒
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float ConvertTime(int value)
        {
            return value * TIME_TO_SECOND;
        }

        /// <summary>
        /// 转换时间秒为单位毫秒
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ConvertTimeBack(float value)
        {
            return (int)(value * TIME_TO_MILLISECOND);
        }
        /// <summary>
        /// 转换厘米到米
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public static float ConvertLength(int value)
        {
            return value * CM_TO_M;
        }

        public static Vector3 ConvertPositionVec(Global_Int_Vector3 value)
        {
            return new Vector3(value.X * CM_TO_M,value.Y* CM_TO_M,value.Z * CM_TO_M);
        }
    }
}
