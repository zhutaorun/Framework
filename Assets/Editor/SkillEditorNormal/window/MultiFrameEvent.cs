using SkillnewConfig;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public partial class FrameEventWindow : EditorWindow
    {
        private FrameEvent StartEvt;
        private FrameEvent EndEvt;
        private EFrameEventType evtType;

        public void SetData(FrameEvent start,FrameEvent end, skillnew_data skill)
        {
            StartEvt = start;
            EndEvt = end;
            skillnew_data = skill;
            evtCount = 2;
            if (start != null && end != null && start.EventType == end.EventType)
                evtType = start.EventType;
            else
                evtType = EFrameEventType.EventTypeNone;
        }

        private void ShowDoubleEventGUI()
        {
            if(evtType == EFrameEventType.EventTypeNone)
            {
                EditorGUILayout.HelpBox("该事件为null",MessageType.Warning);
                return;
            }
            switch(evtType)
            {
                case EFrameEventType.ChangeDir:
                    Show2DirEvent();
                    break;
                case EFrameEventType.ChangeSuperDefence:
                    Show2SuperDefEvent();
                    break;
                case EFrameEventType.PushTargets:
                    Show2PushTargetsEvent();
                    break;
                case EFrameEventType.CameraFollowHeight:
                    Show2CameraHeightEvent();
                    break;
            }
        }

        private void Show2CameraHeightEvent()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            EditorGUILayout.HelpBox("开始配置",MessageType.Info);
            EventTimeGUI(StartEvt);
            CameraHeightGUI(StartEvt);

            EditorGUILayout.HelpBox("结束配置",MessageType.Info);

            EventTimeGUI(EndEvt);
            CameraHeightGUI(EndEvt);
            EditorGUILayout.EndVertical();
        }

        private void Show2DirEvent()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            EditorGUILayout.HelpBox("开始配置", MessageType.Info);
            EventTimeGUI(StartEvt);
            ChangeDirGUI(StartEvt);

            EditorGUILayout.HelpBox("结束配置", MessageType.Info);

            EventTimeGUI(EndEvt);
            ChangeDirGUI(EndEvt);
            EditorGUILayout.EndVertical();
        }

        private void Show2SuperDefEvent()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            EditorGUILayout.HelpBox("开始配置", MessageType.Info);
            EventTimeGUI(StartEvt);
            SuperDefenceGUI(StartEvt);

            EditorGUILayout.HelpBox("结束配置", MessageType.Info);

            EventTimeGUI(EndEvt);
            SuperDefenceGUI(EndEvt);
            EditorGUILayout.EndVertical();
        }

        private void Show2PushTargetsEvent()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            EditorGUILayout.HelpBox("开始配置", MessageType.Info);
            EventTimeGUI(StartEvt);
            ChangePushGUI(StartEvt,true);

            EditorGUILayout.HelpBox("结束配置", MessageType.Info);

            EventTimeGUI(EndEvt);
            ChangePushGUI(EndEvt,false);
            EditorGUILayout.EndVertical();
        }


    }

}