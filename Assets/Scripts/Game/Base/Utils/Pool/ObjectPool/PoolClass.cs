namespace GameFrame.Pool
{
    /// <summary>
    /// 对象池基类
    /// </summary>
    public class PoolClass 
    {
        public IPool Holder = null;
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


        /// <summary>
        /// 启用时
        /// </summary>
        public virtual void OnUse()
        {
            IsOnUse = true;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            if(Holder != null)
            {
                IsOnUse = false;
                OnRelease();
                Holder.Release(this);
                Holder = null;
            }
            else
            {
                Debug.LogError("PoolClass release error!");
            }
        }
    }

}