using System;
using System.Reflection;
using System.Collections.Generic;
namespace GameEditor
{
    public static class CustomGUI
    {
        private static UnityEngine.HideInInspector hideAttr = new UnityEngine.HideInInspector();

        public static Dictionary<FieldInfo, Attribute> fieldDict = new Dictionary<FieldInfo, Attribute>()
        {
            { typeof(SkillConfig.skill_config.FrameEvent).GetField("EventTimePoint"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillConfig.skill_config.FrameEvent).GetField("FrameEventId"),new GameFrame.CustomizeGUIAttribute("GameEditor.FrameEventWindow","FrameEventIdFunc")},
            { typeof(SkillConfig.skill_config.SkillActionInfo).GetField("ActionTime"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillConfig.skill_config.SkillActionInfo).GetField("ActionStartPoint"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillConfig.skill_config.skill_data).GetField("Id"),new GameFrame.CustomizeGUIAttribute("GameEditor.ActionOverview","ShowID")},
            { typeof(SkillConfig.skill_config.effect_data).GetField("Path"),new GameFrame.CustomizeGUIAttribute("GameEditor.EffectWindow","ShowPath")},

        };

        public static Dictionary<PropertyInfo, Attribute> propertyDict = new Dictionary<PropertyInfo, Attribute>() {
            { typeof(SkillnewConfig.FrameEvent).GetProperty("EventTimePoint"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillnewConfig.SkillActionInfo).GetProperty("ActionTime"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillnewConfig.SkillActionInfo).GetProperty("ActionStartPoint"),new GameFrame.CustomizeGUIAttribute("GameEditor.CustomGUI","GUI_Frame")},
            { typeof(SkillnewConfig.skillnew_data).GetProperty("Id"),new GameFrame.CustomizeGUIAttribute("GameEditor.ActionOverview","ShowID")},
            { typeof(EffectConfig.effect_data).GetProperty("Path"),new GameFrame.CustomizeGUIAttribute("GameEditor.EffectWindow","ShowPath")},

        };

        public static Dictionary<Type, GameFrame.CustomizeGUIAttribute> typeDict = new Dictionary<Type, GameFrame.CustomizeGUIAttribute>()
        {
            { typeof(SkillnewConfig.FrameEvent),new GameFrame.CustomizeGUIAttribute("GameEditor.FrameEventWindow","FrameEventFunc")}
        };
    }
}
