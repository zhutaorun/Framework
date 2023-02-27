using GameFrame.Pool;
using System;

namespace GameFrame.Skill.Component
{
    public class Component<T> : IComponents where T: PoolClass,new()
    {
        private T[] _datas = new T[50];
        private int _datasCount = 0;

        public int DataCount { get { return _datasCount;  }private set { } }

        public Component()
        {

        }

        public void OnRemoveEntity(int id)
        {
            if (id > _datasCount) return;

            if(_datas[id] != null)
            {
                _datas[id].Release();
                _datas[id] = null;
            }
        }

        public void Release()
        {
            for(int i=0;i<_datasCount;i++)
            {
                if (_datas[i] != null)
                    _datas[i].Release();
            }
            if (_datasCount > 0)
                Array.Clear(_datas,0,_datasCount);

            _datasCount = 0;
        }

        public T OnAddEntity(int id)
        {
            if(id >= _datasCount)
            {
                if (id >= _datas.Length)
                    Array.Resize(ref _datas,id+ _datas.Length);

                _datasCount = id + 1;
            }

            if(_datas[id] == null)
            {
                _datas[id] = ObjectPool<T>.Get();
            }

            return _datas[id];
        }

        public T GetComponent(int id)
        {
            if (id >= _datasCount || id < 0)
                return null;
            return _datas[id];
        }
    }
}
