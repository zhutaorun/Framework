//using Google.Protobuf.Collections;
//using System.Collections.Generic;

//namespace skillnewConfig
//{
//    public class FrameEvent
//    {
//        public int EventTimePoint;  //触发时间(帧)
//        public EFrameEventType EventType;//事件类型
//        public int FrameEventId;//事件ID
//        public EEventTriggerCondition TriggerEventCondition;//触发条件
//        public List<EventDamage> DamageData;//伤害配置
//        public Global_Int_Vector3 Offset;//特效，子弹，陷阱偏移
//        public int Angle;//角度偏移
//        public bool EffectFixPosition;//特效固定位置释放
//        public bool OnlyMainRole;//是否只有mainrole生效
//    }

//    public enum EFrameEventType
//    {
//        EventTypeNone =0,
//        SendBullet=1,//子弹|发射
//        DirectHurt =2,//直接伤害
//        SetTrap =3,//陷阱
//        SetBuff=4,//buff
//        Effect =5,//显示特效
//        MapItem = 6,//物件
//        ParabolaBullet =7,//子弹|平抛子弹
//        UpParabolaBullet =8,//子弹|上抛子弹
//        PickAlarm = 9,//显示预警
//        Sound = 10,//声音
//        CameraShake  =11,//摄像机|相机抖动
//        ChangeDir = 12,//转向状态
//        ChangeSuperDefence =13,//超级盔甲防御变更
//        CameraBlur = 14,//摄像机|普通模糊
//        ChangeBeHitState =15,//被击状态改变
//        CameraBlurLow = 16,//摄像机|低模糊
//        CameraBlurHigh = 17,//摄像机|高模糊
//        CameraZoom = 18,    //摄像机|缩放
//        SetConnect = 19,    //设置连接
//        PushTargets = 20,   //推怪
//        CameraFollowHeight = 21,//摄像机|高度跟随
//        SetMultipleTargetBullet= 22,//子弹|多个墓壁
//        SetFly = 23,            //设置飞行（1:0飞0：落地）
//        SendCircleBullet = 24,  //子弹|环形子弹
//        LockCameraRotate = 25,  //摄像机|限制旋转
//        CameraHorizontalZoom = 26, //摄像机|水平缩放
//        CameraVerticalZoom = 27,//摄像机|垂直缩放
//        CreateEnergy = 28,//制造能量
//        CameraFollowWeaponX = 29,//摄像机|跟随武器X
//        CameraFollowWeaponY = 30,//摄像机|跟随武器Y
//        SetClientMove = 31, //客户端移动
//        Summmon = 32,   //召唤
//    }

//    public class Global_Int_Vector3
//    {
//        public int X;
//        public int Y;
//        public int Z;
//    }

//    public enum EEventTriggerCondition
//    {
//        ConditonNone = 0,
//        PlayersInAttackArea = 1,//范围内有玩家
//    }

//    public class EventDamage
//    {
//        public int SuperAttack; //超级盔甲攻击
//        public eventpick_data EventPick;//范围
//        public List<OutputItem> Outputs;//输出列表
//    }

//    public class eventpick_data
//    {
//        public int Id;//
//        public EPickShapType EventPickShap;//选择目标AOE形状
//        public string EventPickName;//名字
//        public int EventPickTargetMaxNum;//选择目标最大个数
//        public EPickType EventPickType;//选择目标优先
//        public ETargetType EventPickTargetType;//目标类型
//        public EPickCenterType EventPickCenterType;//AOE中心类型
//        public int EventCenterOffSetX;//中心点偏移X cm
//        public int EventCenterOffeSetZ;//中心点偏移Z cm
//        public EPickDirType EventPickDirType;//方向
//        public int DirOffset;//方向偏移角度
//        public ESkillRangeType EventRangeWidth;//矩形宽
//        public ESkillRangeType EventRangeMinDistance;//攻击最近范围 环小圆半径，扇形近点距离
//        public ESkillRangeType EventRangeMaxDistance;//攻击最远范围 圆、扇形的半径，矩形远边距离，环大圆半径
//        public int EventRangeWidthValue;//矩形宽cm(该值非0就用该值否则就用EventRangeWidth属性值)
//        public int EventRangeMaxDistanceValue;//攻击最远范围(该值非0就用该值否则用EventRangeMaxDitsance属性值)
//        public int EventRangeMinDistanceValue;//攻击最近范围(该值非0就用该值否则用EventRangeMinDitsance属性值)
//        public int EventAngleStart;//开始角度
//        public int EventAngleEnd;//结束角度
//        public bool IgnoreCheckY;//忽略检测高度Y
//        public ECheckYType EventCheckYType;//检测Y的类型
//        public int CheckYRangeValue;//高度范围
//    }

//    public enum EPickShapType
//    {
//        ShapNone =0,//
//        Circle = 1,//圆形
//        RectRanglec= 2,//矩形
//        Sector=3,//扇形
//    }

//    public enum ETargetType
//    {
//        TargetNone =0,//不需要目标
//        Friend=1,//友方
//        Enemy=2,//敌方
//        FriendExcludeMe =3,//友方不包含自己
//        EnemyPlayer =4,//敌方玩家
//    }

//    public enum EPickType
//    {
//        MinDis =0,//优先最近
//        MinBlood =1,//优先血量最少
//        Random=2,//随机
//        Lock=3,//当前锁定目标
//    }

//    public enum EPickCenterType
//    {
//        SelfPos =0,//自身坐标为中心
//        LockTargetPos=1,//锁定目标为中心
//        CmdPos=2,//玩家选择的坐标为中心
//    }

//    public enum EPickDirType
//    {
//        SelfDir = 0,//自身方向
//        TargetDir =1,//目标方向
//        CmdDir = 2,//玩家选择方向

//    }

//    public enum ESkillRangeType
//    {
//        RangeTypeNone =0,//自定义
//        Range =1,//技能属性Range
//        Redius =2,//技能属性Reduis
//        AfterField =3,//技能属性AfterField
//        RangeEx =4,//技能属性RangeEx
//    }

//    public enum ECheckYType
//    {
//        CheckAll =0,//检测上下
//        CheckDown =1,//检测下方
//        CheckUp=2,//检测上方
//    }
//    public class OutputItem
//    {
//        public EOutputType OutputType;//输出类型
//        public int Value;   //对应ID
//        public EOutputTime OutputTime;
//        public int OutputDelayTime;//延迟输出ms
//        public bool NotShow;//不显示
//        public Global_Int_Vector3 Offset;//特效，子弹，陷阱偏移
//        public int Angle;//角度偏移
//    }


//    public enum EOutputTime
//    {
//        OnStart = 0,//开始时
//        OnEnd  = 1,//结束时
//        OnInterval = 2,//频率触发
//        OnBeTrigger= 3,//被触发时
//        OnDurExist = 4,//存在期间
//        OnFixedTime = 5,//固定的时间点触发
//        OnBeTriggerAndNotRemove = 6,//触发且没移除
//    }


//    public enum EOutputType
//    {
//        OptDamage = 0,//伤害公式
//        OptAttr = 1,//属性公式
//        OptBuff = 2,//Buff
//        OptEffect = 3,//特效
//        OptSkillFlag = 4,//技能标记
//        OptTrap = 5,//陷阱
//        OptHitMotion=6,//被击效果
//        OptRandomTrap = 7,//随机陷阱
//        OptCamShake =8,//镜头震动
//        OptBullet = 9,//子弹
//        OptMapItem =10,//场景物件
//        OptTotalDamagePercent = 11,//总伤害百分比
//        OptEneryNum = 12,//技能能量
//    }

//    public class skillnew_data
//    {
//        public int Id;
//        public string SkillName;    //技能名字
//        public uint SkillSound;     //技能音效
//        public ESkillType SkillType;//技能类型
//        public string SkillDescribe;//技能描述
//        public string SkillIconDescribe;//Icon上的描述
//        public string SkillIcon;//Icon路径
//        public int SkillRange;//范围
//        public int CollisoionRedius;//半径
//        public int AttackRangeAfterFired;//技能释放后的范围
//        public int AttackRangeEx; //范围Ex
//        public ETargetType SkillTargetType;//技能锁敌目标类型
//        public ETargetSelectType SelectTargetType;//选择目标的逻辑
//        public bool HaveRootmotion;//是否有运动位移
//        public List<FrameEvent> Evts;//没有动作的技能输出
//        public List<SkillActionInfo> SkillActionList;//动作列表
//    }

//    public enum ESkillType
//    {
//        CommonSkill=0,//普通技能
//        DodgeSkill=1,//闪避技能
//        Attack=2,//普攻
//        Passive =3,//被动
//        Jump=4,//跳跃
//    }

//    public enum ETargetSelectType
//    {
//        LowestHp = 0,//血量最低
//        HighestHp = 1,//血量最高
//    }

//    public class SkillActionInfo
//    {
//        public int ActionTime;//动作时长
//        public int ActionStartPoint;//动作开始点
//        public string ActionPath;//路径
//        public int AnimCrossTime;//与前一动作融合时间ms
//        public EActionOverType ActionOverType;//结束类型
//        public EActionTriggerCondition TriggerActionCondition;//触发条件
//        public ESkillRangeType ActionRangeType;//对应技能范围
//        public int BackCD;//返还CD
//        public RepeatedField<FrameEvent> FrameEvents;//帧事件列表
//        public List<MotionInfo> MotionInfos;//运动坐标
//        public List<int> MotionStartIndex;//动作坐标起始点
//        public int ActionSpeed;//动作播放速度
//    }

//    public enum EActionOverType
//    {
//        EActionOverType_TimeOver = 0,//时间播完
//        EActionOverType_MoveOver = 1,//移动到终点
//        EActionOverType_ActOnDirKeyUp=2,//方向键抬起时，主动结束技能
//        EActionOverType_MoveDirDifLookDir = 3,//非正前方移动（朝向和当前方向差45度以上）
//        EActionOverType_MoveDown =4,//移动落地
//        EActionOverType_KeepingSkillBuffEnd = 5,//持续技能buff结束
//    }

//    public enum EActionTriggerCondition
//    {
//        ActConditionNone =0,//无条件
//        TargetInAttackArea = 1,//范围内有目标
//    }

//    public class MotionInfo
//    {
//        public int MotionTime;//时间单位ms
//        public int PosX;//坐标单位mm
//        public int PosY;//坐标单位mm
//        public int PosZ;//坐标单位mm
//    }

//    public enum EAttri
//    {
//        Attri_BEGIN = 0,
//        Attri_Hp = 1,
//        Attri_MaxHp = 2,
//        Attri_HPRecoverBattle = 3,
//        Attri_MaxMp = 4,
//        Attri_Exp = 5,
//        Attri_ASpeed = 6,
//        Attri_MSpeed = 7,
//        Attri_Item = 8,

//    }

//    public enum ESkillDirType
//    {
//        SkillDirNone = 0,//不能转向
//        Slow = 1,//缓慢转向
//        Immediate = 2,//快速转向
//        MidSpeed = 3,//中速转向
//    }
//}

