namespace DSFramework.Function.Resource
{
    using System.Collections.Generic;
    using UnityEngine;

    public class InstancePool 
    {
        private Dictionary<string, Stack<GameObject>> _allInstances = new Dictionary<string, Stack<GameObject>>();
        private GameObject _instancePoolTransRoot = null;

        public InstancePool()
        {
            GameObject go = GameObject.Find("GameObjectPool");
            if( go== null)
            {
                go = new GameObject("GameObjectPool");
            }
            if(Application.isPlaying)
            {
                Object.DontDestroyOnLoad(go);
            }
            _instancePoolTransRoot = go;
        }


        public GameObject Get(string key)
        {
            Stack<GameObject> objects = null;
            if(!_allInstances.TryGetValue(key,out objects))
            {
                return null;
            }
            else
            {
                if(objects == null || objects.Count== 0)
                {
                    return null;
                }

                GameObject go = objects.Pop();
                InitInst(go,true);
                return go;
            }
        }

        /// <summary>
        /// 获取吃的所有对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<GameObject> GetAll(string key)
        {
            Stack<GameObject> objects = null;
            List<GameObject> objectList = null;
            if(!_allInstances.TryGetValue(key,out objects))
            {
                return null;
            }
            else
            {
                if(objects == null || objects.Count == 0)
                {
                    return null;
                }

                objectList = new List<GameObject>();
                while(objects.Count>0)
                {
                    GameObject go = objects.Pop();
                    InitInst(go,true);
                    objectList.Add(go);
                }
                return objectList;
            }
        }

        /// <summary>
        /// 回收GameObject
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">obj</param>
        /// <param name="forceDstroy">是否强制删除</param>
        public void Recycle(string key,GameObject obj,bool forceDstroy = false)
        {
            //强制删除
            if(forceDstroy)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(obj);
                }
                else
                {
                    GameObject.DestroyImmediate(obj);
                }
                return;
            }

            Stack<GameObject> objects = null;
            if(!_allInstances.TryGetValue(key,out objects))
            {
                objects = new Stack<GameObject>();
                _allInstances[key] = objects;
            }

            InitInst(obj,false);
            objects.Push(obj);
        }


        public void Clear(string key)
        {
            Stack<GameObject> objects = null;
            if(_allInstances.TryGetValue(key,out objects))
            {
                while(objects.Count>0)
                {
                    GameObject objToDestory = objects.Pop();
                    if(Application.isPlaying)
                    {
                        GameObject.Destroy(objToDestory);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(objToDestory);
                    }
                }
            }
        }


        public void ClearAll()
        {
            foreach(var item in _allInstances) 
            {
                while(item.Value.Count>0)
                {
                    GameObject objTpDestroy = item.Value.Pop();
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(objTpDestroy);
                    }
                    else 
                    {
                        GameObject.DestroyImmediate(objTpDestroy);
                    }
                }
            }
            _allInstances.Clear();
        }


        public void InitInst(GameObject inst,bool active = true)
        {
            if(inst != null)
            {
                if (active)
                    inst.SetActive(true);
                else if (inst.activeSelf)
                    inst.SetActive(true);
                inst.transform.SetParent(_instancePoolTransRoot.transform,true);
            }
        }
    }
}