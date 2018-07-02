using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static IDictionary<TKey, TValue> CloningWithValues<TKey, TValue> (this IDictionary<TKey, TValue> original) where TValue : ICloneable
        {
            IDictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();

            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                try
                {
                    ICloneable cloneable = entry.Value as ICloneable;
                    ret.Add(entry.Key, (TValue)cloneable.Clone());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return ret;
        }

        public static IDictionary<Tkey, TNewValue> ConvertValue<Tkey, TValue, TNewValue>(this IDictionary<Tkey, TValue> original, Func<TValue, TNewValue> converter)
        {
            IDictionary<Tkey, TNewValue> ret = new Dictionary<Tkey, TNewValue>();

            foreach (var pair in original)
            {
                ret.Add(pair.Key, converter(pair.Value));
            }

            return ret;
        }

        public static IList<T> CloneWithValues<T>(this IList<T> lst) where T : ICloneable
        {
            List<T> retList = new List<T>();

            foreach (var item in lst)
            {
                retList.Add((T)item.Clone()); 
            }

            return retList;
        }
    }
}
