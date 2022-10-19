using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CollectionUtils
    {
        public static bool ContainsAllKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, IList<TKey> keys)
        {
            foreach (TKey key in keys)
            {
                if (!dict.ContainsKey(key))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
