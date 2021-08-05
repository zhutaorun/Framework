using System;
using UnityEditor;
using UnityEditor.UI;

namespace GameFrame.UI
{

    [CustomEditor(typeof(RichText))]
    [CanEditMultipleObjects]
    public class RichTextEditor : GraphicEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            var serializedObject = this.serializedObject;
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_Atlas = serializedObject.FindProperty("m_Atlas");
            m_SupportCache = serializedObject.FindProperty("m_SupportCache");
            m_SupportVertexCache = serializedObject.FindProperty("m_SupportVertexCache");

            _lpfnPareseText = System.Delegate.CreateDelegate(typeof(Action),serializedObject.targetObject,"_ParseText") as Action;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var currentTextString = m_Text.stringValue;
            if(_lastTextString!=currentTextString)
            {
                _lastTextString = currentTextString;

                var richText = serializedObject.targetObject as RichText;
                if(richText.IsActive() && null!=_lpfnPareseText)
                {
                    _lpfnPareseText();
                }
            }

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(m_Atlas,true);
            EditorGUILayout.PropertyField(m_SupportCache,false);
            EditorGUILayout.PropertyField(m_SupportVertexCache, false);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
        private SerializedProperty m_Atlas;
        private SerializedProperty m_Text;
        private SerializedProperty m_FontData;
        private SerializedProperty m_SupportCache;
        private SerializedProperty m_SupportVertexCache;

        private string _lastTextString;
        private Action _lpfnPareseText;
    }
}
