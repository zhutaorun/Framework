using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace GameFrame.AssetBundles
{

    public class ResourcesMapItem
    {
        public string assetbundleName;
        public string assetName;
    }
    public class AssetsPathMapping
    {
        protected const string PATTREN = AssetBundleConfig.CommonMapPattren;
        protected Dictionary<string, ResourcesMapItem> pathLookup = new Dictionary<string, ResourcesMapItem>();
        protected Dictionary<string, List<string>> assetsLookup = new Dictionary<string, List<string>>();
        protected Dictionary<string, string> assetbundleLookup = new Dictionary<string, string>();
        protected List<string> emptyList = new List<string>();
        
        public AssetsPathMapping()
        {
            AssetName = AssetBundlePath.PackagePathToAssetsPath(AssetBundleConfig.AssetsPathMapFileName);
            AssetbundleName = AssetBundlePath.AssetBundlePathToAssetBundleName(AssetName);
        }

        public string AssetbundleName
        {
            get;
            protected set;
        }

        public string AssetName
        {
            get;
            protected set;
        }

        public void Initialize(string content)
        {
            if(string.IsNullOrEmpty(content))
            {
                Debug.LogError("ResourcesNameMap mepty");
                return;
            }

            content = content.Replace("\r\n","\n");
            string[] mapList = content.Split('\n');
            for (int i = 0, IMax = mapList.Length; i < IMax; i++)
            {
                if (string.IsNullOrEmpty(mapList[i]))
                    continue;
                string[] splitArr = mapList[i].Split(new[] { PATTREN }, System.StringSplitOptions.None);
                if(splitArr.Length<2)
                {
                    Debug.LogError("splitArr Length < 2:"+mapList[i]);
                    continue;
                }

                ResourcesMapItem item = new ResourcesMapItem();
                // 如：ui/prefab/assetbundleupdaterpanel_prefab.assetbundle
                item.assetbundleName = splitArr[0];
                // 如：UI/Prefab/AssetbundleUpdaterPanel.prefab
                item.assetName = splitArr[1];

                //这里assetPath 与 assetName是一样的，使用时要注意
                var assetPath = item.assetName;
                if (!pathLookup.ContainsKey(assetPath))
                {
                    pathLookup.Add(assetPath,item);
                }
                else
                {
                    Debug.LogErrorFormat("pathLookup ContainsKey:{0}",assetPath);
                }
                List<string> assetsList = null;
                assetsLookup.TryGetValue(item.assetbundleName, out assetsList);
                if(assetsList == null)
                {
                    assetsList = new List<string>();
                }
                if (!assetsList.Contains(item.assetName))
                {
                    assetsList.Add(item.assetName);
                }
                assetsLookup[item.assetbundleName] = assetsList;

                if(!assetbundleLookup.ContainsKey(item.assetName))
                {
                    assetbundleLookup.Add(item.assetName,item.assetbundleName);
                }
            }
        }

        /// <summary>
        /// assetPath = "UI/Prefab/View/UILaunch.prefab"
        /// assetbundleName = "UI/Prefab/View/UILaunch.prefab"
        /// assetName = "UI/Prefab/View/UILaunch.prefab"
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetbundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool MapAssetPath(string assetPath,out string assetbundleName,out string assetName)
        {
            assetbundleName = null;
            assetName = null;
            ResourcesMapItem item = null;
            if(pathLookup.TryGetValue(assetPath,out item))
            {
                assetbundleName = item.assetbundleName;
                assetName = item.assetName;
                return true;
            }
            return false;
        }


        public List<string> GetAllAssetNames(string assetbundleName)
        {
            List<string> allAssets = null;
            assetsLookup.TryGetValue(assetbundleName, out allAssets);
            return allAssets == null ? emptyList : allAssets;
        }

        public string GetAssetBundleName(string assetName)
        {
            string assetbundleName = null;
            assetbundleLookup.TryGetValue(assetName, out assetbundleName);
            return assetbundleName;
        }
    }
}
