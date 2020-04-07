using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassPool
{
    /// <summary>
    /// Pool接口
    /// </summary>
    public interface IPool
    {
        void Release(PoolClass obj);
    }
}
