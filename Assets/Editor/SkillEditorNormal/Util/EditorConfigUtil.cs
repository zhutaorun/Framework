using System;

namespace GameEditor
{

    public static class EditorConfigUtil 
    {
        /// <summary>
        /// 毫秒转帧数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int ConvertTime2Frame(int time)
        {
            return (int)Math.Round(time * 0.001f * EditorSkillDisplayManager.FPS);
        }

        public static int ConvertSecond2Frame(float time)
        {
            return (int)Math.Round(time * EditorSkillDisplayManager.FPS);
        }

        public static int ConvertFrame2Time(float frame)
        {
            return (int)(frame * 1000f / EditorSkillDisplayManager.FPS);
        }

        public static int ConvertFrame2Time(int frame)
        {
            return (int)(frame * 1000f / EditorSkillDisplayManager.FPS);
        }

        public static string GetAnimNameByPath(string path)
        {
            int index = path.LastIndexOf("/");
            index++;
            return path.Substring(index,path.Length - index);
        }
    }
}