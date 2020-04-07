using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace GameFrame.AssetBundles
{
    public class AssetBundleConfig
    {
        public const string AssetsFolderName = "Assets";
        public const string AssetsPackageName = "AssetsPackage";

        public const string AssetBundlesFolderName = "assetbundles";

        public const string AssetBundleSuffix = ".assetbundle";
        public const string AssetsPathMapFileName = "assetsmap.bytes";
        public const string BundleManifestFileName = "bundlemanifest.json";

        public const string CommonMapPattren = ",";

        public const string ShaderPathKey = "shader/";
        public const string FontPathKey = "font/";
        public const string AtlasPathKey = "art/ui";
        public const string UiTexturePathKey = "ui/uitexture";


#if UNITY_EDITOR


        private static int mIsEditorMode = -1;
        private const string kIsEditorMode = "IsEditorMode";
        private static int mIsSimulateMode = -1;
        private const string kIsSimulateMode = "IsSimulateMode";
        public static bool IsEditorMode
        {
            get
            {
                if (mIsEditorMode == -1)
                {
                    if(!EditorPrefs.HasKey(kIsEditorMode))
                    {
                        EditorPrefs.SetBool(kIsEditorMode,true);
                    }
                    mIsEditorMode = EditorPrefs.GetBool(kIsEditorMode,true)? 1:0;
                }

                return mIsEditorMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if(newValue != mIsEditorMode)
                {
                    mIsEditorMode = newValue;
                    EditorPrefs.SetBool(kIsEditorMode,value);
                    if(value)
                    {
                        IsSimulateMode = false;
                    }
                }
            }
        }


        public static bool IsSimulateMode
        {
            get
            {
                if(mIsSimulateMode == -1)
                {
                    if (!EditorPrefs.HasKey(kIsSimulateMode))
                    {
                        EditorPrefs.SetBool(kIsSimulateMode,true);
                    }
                    mIsSimulateMode = EditorPrefs.GetBool(kIsSimulateMode,false)?1:0;
                }
                return mIsSimulateMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if(newValue != mIsSimulateMode)
                {
                    mIsSimulateMode = newValue;
                    EditorPrefs.SetBool(kIsSimulateMode,value);

                    if(value)
                    {
                        IsEditorMode = false;
                    }
                }
            }
        }
#endif
    }
}
