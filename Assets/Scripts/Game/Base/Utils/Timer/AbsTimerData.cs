namespace GameFrame
{
    using System;

    internal abstract class AbsTimerData : Pool.PoolClass
    {
        private uint m_nTimerId;

        public uint NTimerId
        {
            get { return m_nTimerId; }
            set { m_nTimerId = value; }
        }

        private int m_nInterval;

        public int NInterval
        {
            get { return m_nInterval; }
            set { m_nInterval = value; }
        }

        private ulong m_unNextTick;
        
        public ulong UnNextTick
        {
            get { return m_unNextTick; }
            set { m_unNextTick = value; } 
        }

        public abstract Delegate Action
        {
            get;
            set;
        }

        public abstract void DoAction();

        public override void OnRelease()
        {
            base.OnRelease();
            m_nTimerId = 0;
            m_nInterval = 0;
            m_unNextTick = 0;
            Action = null;
        }
    }
    /// <summary>
    /// 无参数定时器实体
    /// </summary>

    internal class TimerData : AbsTimerData
    {
        private Action m_action;
        public override Delegate Action
        {
            get { return m_action; }
            set { m_action = value as Action; }
        }

        public override void DoAction()
        {
            if (m_action != null)
                m_action();
        }

        public override void OnRelease()
        {
            base.OnRelease();
            m_action = null;
        }
    }

    /// <summary>
    /// 1个参数定时器实体
    /// </summary>
    /// <typeparam name="T">参数1</typeparam>
    internal class TimerData<T>:AbsTimerData
    {
        private Action<T> m_action;

        public override Delegate Action 
        { 
            get => m_action; 
            set 
            { 
                m_action = value as Action<T>; 
            } 
        }


        private T m_arg1;

        public T Arg1 
        {
            get { return m_arg1; }
            set { m_arg1 = value; }
        }

        public override void DoAction()
        {
            m_action(m_arg1);
        }

        public override void OnRelease()
        {
            base.OnRelease();
            m_action = null;
            m_arg1 = default(T);
        }
    }

    /// <summary>
    /// 2个参数定时器实体
    /// </summary>
    /// <typeparam name="T">参数1</typeparam>
    /// <typeparam name="U">参数2</typeparam>
    internal class TimerData<T,U>: AbsTimerData
    {
        private Action<T, U> m_action;

        public override Delegate Action 
        {
            get { return m_action; }
            set { m_action = value as Action<T, U>; } 
        }

        private T m_arg1;

        public T Arg1
        {
            get { return m_arg1; }
            set { m_arg1 = value; }
        }

        private U m_arg2;

        public U Arg2
        {
            get { return m_arg2; }
            set { m_arg2 = value; }
        }

        public override void DoAction()
        {
            m_action(m_arg1, m_arg2);
        }

        public override void OnRelease()
        {
            base.OnRelease();
            m_action = null;
            m_arg1 = default(T);
            m_arg2 = default(U);
        }
    }

    /// <summary>
    /// 3个参数定时器实体
    /// </summary>
    /// <typeparam name="T">参数1</typeparam>
    /// <typeparam name="U">参数2</typeparam>
    /// <typeparam name="V">参数3</typeparam>
    internal class TimerData<T,U,V>: AbsTimerData
    {
        private Action<T, U,V> m_action;

        public override Delegate Action
        {
            get { return m_action; }
            set { m_action = value as Action<T, U,V>; }
        }

        private T m_arg1;

        public T Arg1
        {
            get { return m_arg1; }
            set { m_arg1 = value; }
        }

        private U m_arg2;

        public U Arg2
        {
            get { return m_arg2; }
            set { m_arg2 = value; }
        }

        private V m_arg3;

        public V Arg3
        {
            get { return m_arg3; }
            set { m_arg3 = value; }
        }

        public override void DoAction()
        {
            m_action(m_arg1, m_arg2,m_arg3);
        }

        public override void OnRelease()
        {
            base.OnRelease();
            m_action = null;
            m_arg1 = default(T);
            m_arg2 = default(U);
            m_arg3 = default(V);
        }
    }

}
