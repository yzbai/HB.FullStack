#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft;

namespace System
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> action)
        {
            if (enumerable == null)
            {
                return;
            }

            foreach (T t in enumerable)
            {
                action(t);
            }
        }

        public static void Add<T>(this IList<T> original, IEnumerable<T> items)
        {
            items.ForEach(t => original.Add(t));
        }

        /// <summary>
        /// CloningWithValues
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        /// <exception cref="System.ArgumentException">Ignore.</exception>
        public static IDictionary<TKey, TValue> CloningWithValues<TKey, TValue>(this IDictionary<TKey, TValue> original) where TValue : ICloneable
        {
            IDictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();

            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ICloneable cloneable = entry.Value;
                ret.Add(entry.Key!, (TValue)cloneable.Clone());
            }
            return ret;
        }

        /// <summary>
        /// CloningWithValues
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        /// <exception cref="System.ArgumentException">Ignore.</exception>
        public static IDictionary<TKey, int> CloningWithValues<TKey>(this IDictionary<TKey, int> original)
        {
            IDictionary<TKey, int> ret = new Dictionary<TKey, int>();

            foreach (KeyValuePair<TKey, int> entry in original)
            {
                ret.Add(entry.Key!, entry.Value);
            }
            return ret;
        }

        /// <summary>
        /// ConvertValue
        /// </summary>
        /// <param name="original"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        /// <exception cref="System.ArgumentException">Ignore.</exception>
        public static IDictionary<TKey, TNewValue> ConvertValue<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> original, Func<TValue, TNewValue> converter)
        {
            IDictionary<TKey, TNewValue> ret = new Dictionary<TKey, TNewValue>();

            foreach (KeyValuePair<TKey, TValue> pair in original)
            {
                ret.Add(pair.Key, converter(pair.Value));
            }

            return ret;
        }

        public static IList<T> CloneWithValues<T>(this IList<T> lst) where T : ICloneable
        {
            List<T> retList = new List<T>();

            foreach (T item in lst)
            {
                retList.Add((T)item.Clone());
            }

            return retList;
        }

        /// <summary>
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        public static bool IsNullOrEmpty<T>([ValidatedNotNull] this IEnumerable<T>? ts)
        {
            return ts == null || !ts.Any();
        }

        /// <summary>
        /// IsNotNullOrEmpty
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        public static bool IsNotNullOrEmpty<T>([ValidatedNotNull] this IEnumerable<T>? ts)
        {
            return ts != null && ts.Any();
        }

        /// <summary>
        /// ToHttpValueCollection
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Ignore.</exception>
        public static NameValueCollection ToHttpValueCollection(this IEnumerable<KeyValuePair<string, string?>> dict)
        {
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString("");

            dict.ForEach(kv => nameValueCollection.Add(kv.Key, kv.Value));

            return nameValueCollection;
        }

        /// <summary>
        /// ToJoinedString
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="seprator"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Ignore.</exception>
        [return: NotNullIfNotNull("ts")]
        public static string? ToJoinedString(this IEnumerable? ts, string seprator)
        {
            if (ts == null)
            {
                return null;
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (object obj in ts)
            {
                stringBuilder.Append(obj.ToString());
                stringBuilder.Append(seprator);
            }

            if (stringBuilder.Length != 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            return stringBuilder.ToString();
        }
    }
}