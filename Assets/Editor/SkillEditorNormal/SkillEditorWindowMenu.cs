using SkillnewConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public static class SkillEditorWindowMenu
    {
        private static Type[] desiredDock = new Type[] { typeof(AnimCurveEditor),typeof(ActionListWindow),typeof(FrameEventWindow) };

        public static Type[] actionDock = new Type[] { typeof(ActionOverview),typeof(ActionSequence) };

        public static Type[] hierachyDock = new Type[] { typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow") };
        public static Type[] inspectorDock = new Type[] { typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow") };

        private static Dictionary<string, EditorWindow> windows;

        static SkillEditorWindowMenu()
        {
            windows = new Dictionary<string, EditorWindow>();
        }

        private static T OpenWindow<T>(Type[] dock) where T : EditorWindow
        {
            string name = typeof(T).Name;
            if(!windows.ContainsKey(name))
            {
                windows.Add(name, EditorWindow.GetWindow<T>(dock));
            }
            else if(windows[name] == null)
            {
                windows[name] = EditorWindow.GetWindow<T>(dock);
            }
            else
            {
                windows[name].Focus();
            }
            return windows[name] as T;
        }

        private static void CloseWindow<T>() where T : EditorWindow
        {
            string name = typeof(T).Name;
            if (windows.ContainsKey(name))
            {
                if (windows[name] != null)
                    windows[name].Close();
                windows.Remove(name);
            }
        }

        public static void CloseAll(EditorWindow except = null)
        {
            foreach(var item in windows)
            {
                if(item.Value != null && item.Value != except)
                {
                    item.Value.Close();
                }
            }
            windows.Clear();
            if(except)
            {
                windows.Add(except.GetType().Name,except);
            }
        }


        public static bool WindowOpened<T>() where T : EditorWindow
        {
            string name = typeof(T).Name;
            return (windows.ContainsKey(name) && windows[name] != null);
        }

        [MenuItem(EditorGlobal.Menu_Root_Design+"技能编辑/主界面 %SM",false,-1)]
        
        public static void OpenMain()
        {
            OpenWindow<CreatureWindow>(hierachyDock);
        }

        public static void OpenActionListWindow()
        {
            OpenWindow<ActionListWindow>(desiredDock);
        }

        public static void OpenActionOverView()
        {
            ActionOverview window = OpenWindow<ActionOverview>(actionDock);
            window.Refresh();
        }

        public static void OpenActionSequence()
        {
            ActionSequence window = OpenWindow<ActionSequence>(actionDock);
        }

        public static void OpenFrameEventWindow(FrameEvent fe,skillnew_data skill,FrameEvent fe2 =null)
        {
            FrameEventWindow window = OpenWindow<FrameEventWindow>(desiredDock);
            if (fe2 != null)
                window.SetData(fe, fe2, skill);
            else
                window.SetData(fe,skill);
        }

        public static void OpenCameraZoomWindow(FrameEvent frameEvent)
        {
            AnimCurveEditor window = OpenWindow<AnimCurveEditor>(desiredDock);
            window.SetFrameEvent(frameEvent);
            window.SetData(frameEvent.FrameEventId);
        }
    }
}
