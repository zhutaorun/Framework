//namespace buff_config
//{
//    public class buff_data
//    {
//        public int Id;
//        public string BuffName;//buff名
//        public int BuffTime;//buff时间ms
//        public EBuffType BuffType;//buff类型
//        public outputattr_data outputattr;  //输出属性
//        public buffskillattr_data outputSkillAttr;  //输出技能属性
//        public int BuffRate;    //buff作用频率(毫秒1次)
//        public int BuffTriggerProbability;//buff触发几率%(0,100]
//        public EuffTriggerCondition BuffTriggerCondition;//buff触发条件
//    }

//    public enum EBuffType
//    {
//        _BEGIN = 0,
//        shield = 1,//护盾
//        taunt = 2,//嘲讽
//        slowdown = 3,//降速
//        silent = 4,//沉默
//        vertigo = 5,//眩晕
//    }

//    public enum EuffTriggerCondition
//    {
//        EuffTriggerCondition_None = 0,
//        EuffTriggerCondition_NotInBattleState = 1,//脱战状态
//        EuffTriggerCondition_InBattleState = 2,//非脱战状态
//    }

//    public class outputattr_data
//    {
//        public SkillnewConfig.EAttri EAttri;    //属性类型
//        public int AttrValue;   //属性值(有上限)
//        public EAttriEffectFormual AttriCalFormula; //属性值计算公式
//        public bool NeedParam; //属性转换(表示百分比输入属性转换成属性 有上限)
//        public SkillnewConfig.EAttri AttrTypeParam; //输入属性类型
//        public int AttrValueParam;//属性参数
//    }

//    public enum EAttriEffectFormual
//    {
//        EAttriEffectFormual_Add = 0,//加
//        EAttriEffectFormual_MulAdd = 1,//加乘百分比
//        EAttriEffectFormual_AdditiveMulAdd = 2,//叠乘
//    }

//    public class buffskillattr_data
//    {
//        public int skillAttrType;//属性类型
//        public int AttrValue;//属性值
//        public int DetialParam;//作为属性参数
//        public EAttriEffectFormual SkillAttriCalFormula;//属性值计算公式
//    }
//}
