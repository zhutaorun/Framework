using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Skill
{

    public class SkillModuleIDManager
    {

        //管理外部ID和当前模块ID的转换
        private Dictionary<uint, int> _idDic = new Dictionary<uint, int>();

        //被回收的Entity列表(存储的是总列表里的索引)
        private Queue<int> _reserverEntities = new Queue<int>(50);

        private int _curLocalIdBase = 0;//当前本地id基础值

        private List<int> _moduleOnlyID = new List<int>(50);

        public void Init()
        {

        }

        public void Clear()
        {
            _idDic.Clear();
            _reserverEntities.Clear();
            _curLocalIdBase = 0;
            _moduleOnlyID.Clear();
        }

        /// <summary>
        /// 获取本地Id没有时申请一个ID
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public int ReqModuleId(uint objectId)
        {
            int id = -1;
            if(_idDic.TryGetValue(objectId,out id))
            {
                return id;
            }
            if(_reserverEntities.Count>0)
            {
                id = _reserverEntities.Dequeue();
            }
            else
            {
                id = _curLocalIdBase;
                _curLocalIdBase++;
            }
            _idDic.Add(objectId,id);
            return id;
        }

        public int ReqModuleId()
        {
            int id = -1;
            if(_reserverEntities.Count>0)
            {
                id = _reserverEntities.Dequeue();
            }
            else
            {
                id = _curLocalIdBase;
                _curLocalIdBase++;
            }
            _moduleOnlyID.Add(id);
            return id;
        }

        public void ReleaseModuleId(int id)
        {
            if(_idDic.ContainsValue(id))
            {
                foreach(var value in _idDic)
                { 
                    if(value.Value == id)
                    {
                        _idDic.Remove(value.Key);
                        break;
                    }
                }
            }
            if (_moduleOnlyID.Contains(id))
                _moduleOnlyID.Remove(id);
            _reserverEntities.Enqueue(id);
        }


        public int GetModuleId(uint objectId)
        {
            int id = -1;
            if (_idDic.TryGetValue(objectId, out id))
                return id;
            return -1;
        }

        public uint GetExternalID(int id)
        {
            foreach(var item in _idDic)
            {
                if(item.Value == id)
                {
                    return item.Key;
                }
            }
            Debug.LogError("GetExternalId Error!"+id);
            return 0;
        }
    }

}