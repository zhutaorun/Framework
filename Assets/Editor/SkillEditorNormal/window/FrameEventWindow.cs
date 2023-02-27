using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SkillnewConfig;
using ShakeConfig;
using BuffConfig;
using BulletConfig;
using TrapConfig;

namespace GameEditor
{
    public partial class FrameEventWindow : EditorWindow
    {
        public static FrameEventWindow Instance = null;
        public void SetData(FrameEvent fe,skillnew_data skill)
        {
            fevent = fe;
            skillnew_data = skill;
            evtCount = 1;
        }

        private FrameEvent fevent = null;
        private skillnew_data skillnew_data = null;
        public bool IsClicked = false;
        private Vector2 scrollPos = Vector2.zero;
        private int evtCount = 1;

        private void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent("FrameEvent","d_CustomTool");
            Instance = this;
        }

        private void OnGUI()
        {
            if(evtCount == 1)
            {
                if (fevent == null) return;
                else if(fevent.EventType == EFrameEventType.CameraZoom || fevent.EventType == EFrameEventType.CameraHorizontalZoom || fevent.EventType == EFrameEventType.CameraVerticalZoom)
                {
                    if(GUILayout.Button("AnimCurve Window"))
                    {
                        SkillEditorWindowMenu.OpenCameraZoomWindow(fevent);
                    }
                }
                ShowOneFrameEventData();
                
            }
            else if (evtCount == 2)
            {
                ShowDoubleEventGUI();
            }
        }

        #region 技能范围计算

        private static void EventTimeGUI(FrameEvent frameEvt)
        {
            frameEvt.EventTimePoint = EditorConfigUtil.ConvertFrame2Time(AttrGUIHelper.GUI_Int_Btn_Attr<FrameEvent>
                ("EventTimePoint",EditorConfigUtil.ConvertTime2Frame(frameEvt.EventTimePoint)));
        }

        public static FrameEvent FrameEventFunc(FrameEvent fevent,int space)
        {
            if (fevent == null) return null;
            GUILayout.BeginHorizontal();
            GUILayout.Space(space);
            GUILayout.BeginVertical();
            switch(fevent.EventType)
            {
                case EFrameEventType.Sound:
                    SoundEventGUI(fevent);
                    break;
                case EFrameEventType.SendBullet:
                case EFrameEventType.SendCircleBullet:
                    BulletGUI(fevent);
                    break;
                case EFrameEventType.CameraShake:
                    CameraShakeGUI(fevent);
                    break;
                case EFrameEventType.ChangeDir:
                    ChangeDirGUI(fevent);
                    break;
                case EFrameEventType.ChangeSuperDefence:
                    SuperDefenceGUI(fevent);
                    break;
                case EFrameEventType.PushTargets:
                    ChangePushGUI(fevent);
                    break;
                case EFrameEventType.SetFly:
                    ChangeSetFlyGUI(fevent);
                    break;
                case EFrameEventType.ChangeBeHitState:
                    ChangeBeHitStateGUI(fevent);
                    break;
                case EFrameEventType.CameraBlur:
                case EFrameEventType.CameraBlurLow:
                case EFrameEventType.CameraBlurHigh:
                    CameraEventTypeGUI(fevent);
                    break;
                case EFrameEventType.CameraFollowWeaponX:
                case EFrameEventType.CameraFollowWeaponY:
                    CameraFollowWeaponGUI(fevent);
                    break;
                case EFrameEventType.LockCameraRotate:
                    LockCameraRotateGUI(fevent);
                    break;
                case EFrameEventType.CreateEnergy:
                    CreateEnergyGUI(fevent);
                    break;
                default:
                    DefaultEventTypeGUI(fevent);
                    break;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            return fevent;
        }

        private static void ChangeBeHitStateGUI(FrameEvent evt)
        {
            
        }

        private static void CameraEventTypeGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("帧数",fevent.FrameEventId);
            fevent.OnlyMainRole = AttrGUIHelper.GUI_Bool_Attr<FrameEvent>("是否只有MainRole生效",fevent.OnlyMainRole);
        }

        private static void CameraFollowWeaponGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("帧数", fevent.FrameEventId);
        }
        
        private static void LockCameraRotateGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("帧数",fevent.FrameEventId);
        }

        private static void DefaultEventTypeGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("FrameEventId",fevent.FrameEventId);
            switch(fevent.EventType)
            {
                case EFrameEventType.SetBuff:
                    var buff = EditorHook.configCtrl.GetData<buff_data>(fevent.FrameEventId);
                    if(buff !=null)
                    {
                        if(GUILayout.Button("保存Buff配置"))
                        {
                            var _data = new Data<buff_data,BuffConfig.container>(SkillDefine.BuffConfigPath, "Id", "BuffName");
                            _data.ReloadConfig(false);
                            _data.SaveConfig();
                        }
                        GUILayout.Label((buff as buff_data).BuffName);
                        GUILayout.Space(10);
                        AttrGUIHelper.GUI_Draw<buff_data>(buff,$"ID:{buff.Id} Name:{buff.BuffName}");
                    }
                    break;
                case EFrameEventType.SetTrap:
                    fevent.Offset = AttrGUIHelper.GUI_IntVector3<FrameEvent>("Offset", fevent.Offset);
                    fevent.Angle = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("Angle",fevent.Angle);
                    var trap = EditorHook.configCtrl.GetData<trap_data>(fevent.FrameEventId);
                    if(trap!= null)
                    {
                        GUILayout.Label((trap as trap_data).Name);
                    }
                    break;

                case EFrameEventType.SendBullet:
                case EFrameEventType.SendCircleBullet:
                    fevent.Offset = AttrGUIHelper.GUI_IntVector3<FrameEvent>("Offset",fevent.Offset);
                    fevent.Angle = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("Angle",fevent.Angle);
                    var bullet = EditorHook.configCtrl.GetData<bullet_data>(fevent.FrameEventId);
                    if (bullet != null)
                    {
                        GUILayout.Label((bullet as bullet_data).Name);
                    }
                    break;

                case EFrameEventType.Effect:
                    fevent.Offset = AttrGUIHelper.GUI_IntVector3<FrameEvent>("Offset",fevent.Offset);
                    fevent.Angle = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("Angle",fevent.Angle);
                    fevent.EffectFixPosition = AttrGUIHelper.GUI_Bool_Attr<FrameEvent>("EffectFixPostion",fevent.EffectFixPosition);
                    var effect = EditorHook.configCtrl.GetData<EffectConfig.effect_data>(fevent.FrameEventId);
                    if(effect!= null)
                    {
                        if(GUILayout.Button("保存Effect配置"))
                        {
                            var _data = new Data<EffectConfig.effect_data, EffectConfig.container>(SkillDefine.EffectConfigPath,"Id","Name");
                            _data.ReloadConfig(false);
                            _data.SaveConfig();
                        }
                        GUILayout.Label((effect as EffectConfig.effect_data).Name);
                        GUILayout.Space(10);
                        AttrGUIHelper.GUI_Draw<EffectConfig.effect_data>(effect,$"ID:{effect.Id} Name:{effect.Name}");
                       
                    }
                    break;
            }
        }

        private static void CameraHeightGUI(FrameEvent frameEvt)
        {
            bool value = AttrGUIHelper.GUI_Bool_Attr<FrameEvent>("摄像机高度跟随",frameEvt.FrameEventId>0);
            frameEvt.FrameEventId = value ? 1 : 0;
        }

        private static void ChangeDirGUI(FrameEvent frameEvt)
        {
            ESkillDirType dirType = frameEvt.FrameEventId == 0 ? ESkillDirType.SkillDirNone : (ESkillDirType)frameEvt.FrameEventId;
            frameEvt.FrameEventId = (int)(ESkillDirType)AttrGUIHelper.GUI_Enum_Attr("转向",dirType,typeof(FrameEvent));
        }

        private static void SuperDefenceGUI(FrameEvent frameEvt)
        {
            frameEvt.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("超级盔甲防御值",frameEvt.FrameEventId);
        }

        private static void CreateEnergyGUI(FrameEvent frameEvent)
        {
            frameEvent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("产出能量值",frameEvent.FrameEventId);
        }

        private static void ChangePushGUI(FrameEvent frameEvt,bool start = false)
        {
            if (start)
                frameEvt.FrameEventId = 1;
            else
                frameEvt.FrameEventId = 0;
        }

        private static void ChangeSetFlyGUI(FrameEvent frameEvt,bool start = false)
        {
            frameEvt.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("飞行1落地0", frameEvt.FrameEventId);
        }
        private static void SoundEventGUI(FrameEvent fevent)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("声音事件:");
            GUILayout.Label(((uint)fevent.FrameEventId).ToString());

            //todo 需要接入Wwise
            //List<AkWwiseProjectData.EventWorkUnit> workUnits = AKWwiseProjectInfo.GetData().EventWwu;
            //bool found = false;
            //if((uint)fevent.FrameEventId>0)
            //{
            //    foreach(var item in workUnits)
            //    {
            //        List<AkWwiseProjectData.Event> events = item.List;
            //        foreach(var e in events)
            //        {
            //            if(e.Id == fevent.FrameEventId)
            //            {
            //                GUILayout.Label(e.Name, GUILayout.Width(150));
            //                found = true;
            //                break;
            //            }
            //        }
            //        if (found)
            //            break;
            //    }
            //}
            //if(EditorGUILayout.DropdownButton(new GUIContent("选择"),FocusType.Passive,GUILayout.Width(150)))
            //{
            //    GenericMenu menu = new GenericMenu();
            //    foreach(var item in workUnits)
            //    {
            //        List<AkWwiseProkectData.Event> events = item.List;
            //        foreach(var e in events)
            //        {
            //            menu.AddItem(new GUIContent(e.Path.Replace("\\", "/")), false, (GenericMenu.MenuFunction)(() =>
            //               {
            //                   fevent.FrameEventId = (int)e.Id;
            //               }));
            //        }
            //    }
            //    menu.ShowAsContext();
            //}

            GUILayout.EndHorizontal();

            //if(found)
            //{
            //    GUILayout.BeginHorizontal();
            //    if(GUILayout.Button("播放",GUILayout.Width(150)))
            //    {
            //        Manager.Sound.PlayGlobalSound((uint)fevent.FrameEventId);
            //    }
            //    if(GUILayout.Button("停止",GUILayout.Width(150)))
            //    {
            //        AkSoundEngine.StopAll();
            //    }
            //    GUILayout.EndHorizontal();
            //}
        }

        private static void BulletGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("FrameEventId",fevent.FrameEventId);
            if(EditorGUILayout.DropdownButton(new GUIContent("选择子弹"),FocusType.Passive,GUILayout.Width(150)))
            {
                List<object> bullets = EditorHook.configCtrl.GetListData<bullet_data>();
                if (bullets == null) return;
                GenericMenu menu = new GenericMenu();
                foreach(var item in bullets)
                {
                    bullet_data bdata = item as bullet_data;
                    menu.AddItem(new GUIContent(bdata.Id + bdata.Name), false, (GenericMenu.MenuFunction)(()=> 
                    {
                        fevent.FrameEventId = (int)bdata.Id;
                    }));
                }
                menu.ShowAsContext();
            }

            fevent.Offset = AttrGUIHelper.GUI_IntVector3<FrameEvent>("Offset", fevent.Offset);
            fevent.Angle = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("Angle",fevent.Angle);
        }


        private static void CameraShakeGUI(FrameEvent fevent)
        {
            fevent.FrameEventId = AttrGUIHelper.GUI_Int_Attr<FrameEvent>("FrameEventId",fevent.FrameEventId);
            if(EditorGUILayout.DropdownButton(new GUIContent("选择摄像机震动配置"),FocusType.Passive,GUILayout.Width(150)))
            {
                List<object> bullets = EditorHook.configCtrl.GetListData<shake_data>();
                if (bullets == null) return;
                GenericMenu menu = new GenericMenu();
                foreach (var item in bullets)
                {
                    shake_data bdata = item as shake_data;
                    menu.AddItem(new GUIContent(bdata.MetaId.ToString()), false, (GenericMenu.MenuFunction)(() =>
                    {
                        fevent.FrameEventId = (int)bdata.MetaId;
                    }));
                }
                menu.ShowAsContext();
            }
        }
        #endregion

        private void ShowOneFrameEventData()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            AttrGUIHelper.GUI_Draw(fevent,"FrameEvent");
            if(fevent.EventType == EFrameEventType.DirectHurt || fevent.EventType == EFrameEventType.SetConnect || fevent.EventType == EFrameEventType.SetMultipleTargetBullet)
            {
                if(fevent.DamageData.Count<=0)
                {
                    if(GUILayout.Button("添加伤害范围数据"))
                    {
                        fevent.DamageData.Add(new EventDamage());
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if(GUILayout.Button("-",GUILayout.Width(20),GUILayout.Height(20)))
                    {
                        fevent.DamageData.Clear();
                    }
                    if(fevent.DamageData.Count>0)
                    {
                        if(AttrGUIHelper.GUI_Draw_Breviary(fevent.DamageData[0],"伤害范围配置",0))
                        {
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            EditorGUILayout.BeginVertical();
                            AttrGUIHelper.GUI_Draw(fevent.DamageData[0],"EventDamage");
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndScrollView();
        }

        private void OnDisable()
        {
            fevent = null;
        }
    }

}