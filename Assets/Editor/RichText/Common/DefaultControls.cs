﻿using UnityEngine;


namespace GameFrame
{
    public static class DefaultControls 
    {
        public struct Resources
        {
            public Sprite standard;
            public Sprite background;
            public Sprite inputField;
            public Sprite knob;
            public Sprite checkmark;
            public Sprite dropdown;
            public Sprite mask;
        }


        private const float kWidth = 100f;
        private const float kThickHeight = 30f;
        private const float kThinHeight = 20f;


        //Helper methods at top CreateUIElementRoot
        public static GameObject CreateUIElementRoot(string name,Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name,GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child,GameObject parent)
        {
            if (parent == null)
                return;
            child.transform.SetParent(parent.transform,false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go,int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for(int i=0;i<t.childCount;i++)
            {
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
            }
        }
    }
}
