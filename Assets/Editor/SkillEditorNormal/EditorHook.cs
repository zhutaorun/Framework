using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEditor
{
    public class EditorHook
    {
        public static GameFrame.Utils.IDescription description;
        public static SkillDataManager skillDataManager;
        public static EditorSkillDisplayManager editorSkillDisplayManager;
        public static ConfigCtrl configCtrl;
        public static EditorModelCtrl editorModelCtrl;

        public static bool initialized = false;

        public static void Initialize()
        {
            description = ProtoDescription.Instance;
            description.LoadFolder();
            skillDataManager = new SkillDataManager();
            editorSkillDisplayManager = new EditorSkillDisplayManager();
            configCtrl = new ConfigCtrl();
            editorModelCtrl = new EditorModelCtrl();
            initialized = true;
        }

        public static void Release()
        {
            if (description != null)
                description.Clear();
            skillDataManager.Dispose();
            editorSkillDisplayManager.Dispose();
            configCtrl.Dispose();
            editorModelCtrl.Dispose();
            initialized = false;
        }

    }
}
