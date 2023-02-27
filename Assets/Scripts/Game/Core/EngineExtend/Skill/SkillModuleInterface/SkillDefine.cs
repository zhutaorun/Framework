
public static class SkillDefine 
{
    #region debug开关
    /// <summary>
    /// 开启特效
    /// </summary>
    public const bool DEBUG_OPEN_EFFECT = true;

    public const bool DEBUG_SHOW_TRAP_RANGE = false;

    #endregion
    #region 路径
    public const string ConfigRoot = "Assets/AssetsPackage/Config";
    public const string JsonConfig = "TableData/json/";
    public const string BytesConfig = "TableData/bytes/";
    public const string SkillConfigPath = "AssetsPackage/Config/TableData/bytes/skill@skillnew.bytes";
    public const string UnitConfigPath = "AssetsPackage/Config/TableData/bytes/unit@unit.bytes";
    public const string SeparatedSkillFolder = "Editor/SkillEditorNormal/SeparatedConfig";
    public const string EffectConfigPath = "AssetsPackage/Config/TableData/bytes/effect@effect.bytes";
    public const string BulletConfigPath = "AssetsPackage/Config/TableData/bytes/bullet@bullet.bytes";
    public const string BuffConfigPath = "AssetsPackage/Config/TableData/bytes/buff@buff.bytes";
    public const string ShakeConfigPath = "AssetsPackage/Config/TableData/bytes/shake@shake.bytes";
    #endregion

    /// <summary>
    /// 动作位移数据路径
    /// </summary>
    public const string ROOTMOTION_DATA_PATH = "Assets/ArtRoot/EditorAnimation/root_motion_data.asset";

    /// <summary>
    /// 特效路径
    /// </summary>
    public const string EffectPath = "Assets/AssetsPackage/ArtExport/Effect/";
    /// <summary>
    /// 特效路径
    /// </summary>
    public const string EffectPath_Runtime = "Assets/AssetsPackage/ArtExport/Effect/";

    /// <summary>
    /// 地面高度
    /// </summary>
    public const float MAP_FLOOR = 0.0f;
    #region 
    public const int FRAMENUM_PERSECOND = 30;
    public static readonly string[] DodgeArray = { "",""};
    #endregion
}
