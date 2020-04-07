using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassPool
{
    public class PoolClass
    {
        public IPool holder = null;
        public bool IsOnUse = false;

        public PoolClass()
        {

        }

        /// <summary>
        /// 回收时
        /// </summary>
        public virtual void OnRelease()
        {
            IsOnUse = false;
        }

        /*启用时*/

        public virtual void OnUse()
        {
            IsOnUse = true;
        }
        public void Release()
        {
            if(this.holder != null)
            {
                IsOnUse = false;
                this.OnRelease();
                this.holder.Release(this);
                this.holder = null;
            }
            else
            {
                UnityEngine.Debug.LogError("PooledClass release error!");
            }
        }
    }
}
