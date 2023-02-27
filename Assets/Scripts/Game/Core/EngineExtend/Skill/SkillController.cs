using GameFrame;
using GameFrame.Skill.Component;
using GameFrame.Skill.GameSystem;
using GameFrame.Skill;
public static class SkillController
{
    /// <summary>
    /// 模块系统管理
    /// </summary>
    public static SystemManager System = new SystemManager();

    /// <summary>
    /// 模块数据管理
    /// </summary>
    public static ComponentManager Component = new ComponentManager();

    /// <summary>
    /// 模块配置管理
    /// </summary>
    public static SkillConfigManager SkillCfg = new SkillConfigManager();

    /// <summary>
    /// 模块ID管理
    /// </summary>
    public static SkillModuleIDManager ID = new SkillModuleIDManager();

    /// <summary>
    /// 输出
    /// </summary>
    public static SkillPlayOutput Output = new SkillPlayOutput();

    public static void Init()
    {
        ID.Init();
        System.Init();
        Component.Init();
        SkillCfg.Initialize();
    }

    public static void Clear()
    {
        ID.Clear();
        System.Clear();
        Component.Clear();
        SkillCfg.ClearAll();
    }
}
