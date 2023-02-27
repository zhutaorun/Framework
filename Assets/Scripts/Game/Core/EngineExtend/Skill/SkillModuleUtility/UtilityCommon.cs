using UnityEngine;

namespace GameFrame.Skill.Utility
{
    public static class UtilityCommon
    {
        public static Transform FindChildByName(string strName,Transform trans)
        {
            if(trans.name == strName)
            {
                return trans;
            }

            Transform ret = null;
            for(int i=0;i<trans.childCount;i++)
            {
                ret = FindChildByName(strName, trans.GetChild(i));
                if(ret != null)
                {
                    return ret;
                }
            }
            return null;
        }

        public static float GetVerticalStartSpeed(float height,ref float verticalAcc,int verticalTime)
        {
            if (height == 0)
                return 0;
            if (verticalTime == 0) verticalTime = 200;

            float timeMin = 1.0f;
            //下落时间
            float t2 = verticalTime * 0.0005f;
            //上升时间
            float t1 = t2;
            if (t2 < timeMin) t2 = timeMin;
            if (t1 < timeMin) t1 = timeMin;
            verticalAcc = height * 2 / (t2 * t2);

            float v0 = (height + verticalAcc * t1 * t1 / 2) / t1;

            return v0;
        }
    }

}