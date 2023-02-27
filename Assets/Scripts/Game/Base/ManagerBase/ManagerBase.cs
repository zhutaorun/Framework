using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{

    public interface IUpdate
    {
        void Update();
    }

    public interface ILateUpdate
    {
        void LateUpdate();
    }

    public interface IFixedUpdate
    {
        void FixedUpdate();
    }
    /// <summary>
    /// 管理器基类
    /// </summary>
    public class ManagerBase 
    {
       public bool IsInitialize { get; private set; }

        public ManagerBase()
        {

        }

        public virtual void Initialize()
        {
            IsInitialize = true;
        }

        public void Destroy()
        {
            OnDestroy();
        }

        public virtual void OnDestroy()
        {

        }
    }
}
