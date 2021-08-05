using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    internal static class ExtendedICollection
    {
        public static bool IsNullOrEmptyEx<T> (this ICollection<T> collection)
        {
            return null == collection || collection.Count == 0;
        }
    }
}
