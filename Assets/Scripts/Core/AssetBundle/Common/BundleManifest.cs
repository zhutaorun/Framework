using UnityEngine;
using System.Collections.Generic;

namespace GameFrame.AssetBundles
{
    [System.Serializable]
    public class BundleManifest
    {
        #region
        public string Platform = string.Empty;

        public int TotalCount = 0;
        public long TotalSize = 0;
        public List<AssetInfo> BundleList = new List<AssetInfo>();
        #endregion


        #region Runtime Store Datas
        [System.NonSerialized]
        private string[] emptyStringArray = new string[] { };
        // "ab name" 与 "ab index" 对应表，为了减少GC
        [System.NonSerialized]
        private Dictionary<string, int> bundleIndexDic;

        public Dictionary<string, int> BundleIndexDic
        {
            get
            {
                if (bundleIndexDic == null)
                    bundleIndexDic = new Dictionary<string, int>();
                for(int i=0,iMax=BundleList.Count;i<iMax;++i)
                {
                    bundleIndexDic.Add(BundleList[i].AssetBundleName,i);
                }
                return bundleIndexDic;
            }
        }


        //ab 缓存包：所有目前已经加载的ab包。包括临时ab包与公共ab包
        [System.NonSerialized]
        private HashSet<int> loadBundleList = new HashSet<int>();
        public HashSet<int> LoadBundleList { get { return loadBundleList; } }

        #endregion

        /// <summary>
        /// 初始化需要外部执行
        /// </summary>
        public void Initialize()
        {

        }

        public void CleanUp()
        {
            for (int i = 0, iMax = loadBundleList.Count; i<iMax; i++)
            {
                UnLoadAssetBundle(i);
            }
            loadBundleList.Clear();
            for (int i = 0, iMax = BundleList.Count; i < iMax; i++)
            {
                BundleList[i].IsResident = false;
                BundleList[i].RefCount = 0;
            }
        }

        #region Bundle Catch
        public bool IsAssetBundleLoaded(int index)
        {
            return loadBundleList.Contains(index);
        }


        public AssetBundle GetAssetBundleCache(int index)
        {
            AssetInfo info = GetAssetInfo(index);
            if (info != null)
            {
                return info.AssetBundle;
            }
            return null;
        }

        public void RemoveAssetBundleCache(int index)
        {
            AssetInfo info = GetAssetInfo(index);
            if(info!=null)
            {
                info.AssetBundle = null;
                loadBundleList.Remove(index);
            }
        }


        public void AddAssetBundleCache(int index,AssetBundle assetBundle)
        {
            AssetInfo info = GetAssetInfo(index);
            if (info != null)
            {
                info.AssetBundle = assetBundle;
                loadBundleList.Add(index);
            }
        }
        #endregion

        #region BundleResident

        public void SetAssetBundleResident(int bundleIndex, bool resident)
        {
            AssetInfo info = GetAssetInfo(bundleIndex);
            if(info !=null)
            {
                info.IsResident = resident;
            }
        }

        public bool IsAssetBundleResident(int index)
        {
            AssetInfo info = GetAssetInfo(index);
            if(info!=null)
            {
                return info.IsResident;
            }
            return false;

        }

        public void SetAssetBundleAlwaysNotUnload(int bundleIndex, bool isAlwaysNotUnload)
        {
            AssetInfo info = GetAssetInfo(bundleIndex);
            if(info!=null)
            {
                info.IsAlwaysNotUnload = isAlwaysNotUnload;
            }
        }
        public bool IsAssetBundleAlwaysNotUnload(int index)
        {
            AssetInfo info = GetAssetInfo(index);
            if(info!=null)
            {
                return info.IsAlwaysNotUnload;
            }
            return false;
        }
        #endregion

        #region Reference Count
        public int GetReferenceCount(int bundleIndex)
        {
            AssetInfo info = GetAssetInfo(bundleIndex);
            if(info != null)
            {
                return info.RefCount;
            }
            return 0;
        }

        public int IncreaseReferenceCount(int bundleIndex)
        {
            AssetInfo info = GetAssetInfo(bundleIndex);
            if(info!=null)
            {
                info.RefCount++;
                return info.RefCount;
            }
            return 0;
        }

        public int DecreaseReferenceCount(int bundleIndex)
        {
            AssetInfo info = GetAssetInfo(bundleIndex);
            if (info != null)
            {
                info.RefCount--;
                if(info.RefCount<0)
                {
                    info.RefCount = 0;
                }
                return info.RefCount;
            }
            return 0;
        }


        #region Help Methods
        /// <summary>
        /// 获取Bundle索引
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public int GetAssetBundleIndex(string assetbundleName)
        {
            int index;
            if(BundleIndexDic.TryGetValue(assetbundleName,out index))
            {
                return index;
            }
            else
            {
                Debug.LogError("Get assetBundleIndex by assetBundleName " + assetbundleName+"is null !");
                return -1;
            }
        }


        public AssetInfo GetAssetInfo(string assetbundleName)
        {
            int index = GetAssetBundleIndex(assetbundleName);
            if(index!=-1)
            {
                return BundleList[index];
            }
            else
            {
                Debug.LogError("Get assetInfo by assetBundleName" + assetbundleName +"is Null!");
                return null;
            }
        }


        public AssetInfo GetAssetInfo(int index)
        {
            if (index >= 0 && index < TotalCount)
            {
                return BundleList[index];
            }
            else
            {
                Debug.LogError("Get AssetInfo by index" +index +"is NUll !");
                return null;
            }
        }


        public void RemoveAssetInfo(string assetbundleName)
        {
            AssetInfo assetInfo = GetAssetInfo(assetbundleName);
            if(assetInfo !=null)
            {
                BundleList.Remove(assetInfo);
            }
        }

        public string[] GetAllAssetBundleNames()
        {
            List<string> tempBundleNames = new List<string>();
            for(int i=0,Imax = BundleList.Count;i<Imax;i++)
            {
                if(!tempBundleNames.Contains(BundleList[i].AssetBundleName))
                {
                    tempBundleNames.Add(BundleList[i].AssetBundleName);
                }
            }
            return tempBundleNames.ToArray();
        }

        public string[] GetAllDependencies(string assetBundleName)
        {
            AssetInfo assetInfo = GetAssetInfo(assetBundleName);
            return assetInfo == null ? emptyStringArray : assetInfo.Dependencies;
        }

        public string[] GetAllDependencies(int index)
        {
            AssetInfo assetInfo = GetAssetInfo(index);
            return assetInfo == null ? emptyStringArray : assetInfo.Dependencies;
        }


        public void UnLoadAssetBundle(int index,bool unloadAllLoadedObjects= false)
        {
            if(index>=0&& index<TotalCount)
            {
                if (BundleList[index].AssetBundle != null)
                {
                    BundleList[index].AssetBundle.Unload(unloadAllLoadedObjects);
                    BundleList[index].AssetBundle = null;
                }
            }
        }

        #endregion
        #endregion

        [System.Serializable]

        public class AssetInfo
        {
            #region Buid  Gen Datas

            //assetbundle名称（AssetPackga相对路径）
            public string AssetBundleName = string.Empty;

            //依赖Bundles
            public string[] Dependencies = new string[] { };

            //被其他Bundle依赖的数量
            public int BeDependedOnCount = 0;
            #endregion

            #region Runtime Store Datas
            //缓存的AssetBundle
            [System.NonSerialized]
            public AssetBundle AssetBundle;

            //是否常驻内存：需要手动添加公共ab包进来，常驻包不会自动卸载（即使引用计数为0），引用计数为0的时可以手动卸载
            [System.NonSerialized]
            public bool IsResident = false;

            //是否永远不卸载：字体，图集的AB包永不卸载
            [System.NonSerialized]
            public bool IsAlwaysNotUnload = false;

            //ab缓存包引用计数：卸载ab包时只有引用计数为0时才会真正执行卸载
            [System.NonSerialized]
            public int RefCount = 0;
            #endregion

        }
    }
}
