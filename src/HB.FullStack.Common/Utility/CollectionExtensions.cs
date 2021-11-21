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
        //public static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> action)
        //{
        //    if (enumerable == null)
        //    {
        //        return;
        //    }

        //    foreach (T t in enumerable)
        //    {
        //        action(t);
        //    }
        //}

        public static void AddRange<T>(this IList<T> ts, IEnumerable<T> items)
        {
            if (ts == null) throw new ArgumentNullException(nameof(ts));

            if (ts is List<T> lst)
            {
                lst.AddRange(items);
                return;
            }

            foreach (var t in items)
            {
                ts.Add(t);
            }
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? ts)
        {
            return ts == null || !ts.Any();
        }

        /// <summary>
        /// IsNotNullOrEmpty
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>

        public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? ts)
        {
            return ts != null && ts.Any();
        }

        /// <summary>
        /// ToHttpValueCollection
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>

        public static NameValueCollection ToHttpValueCollection(this IEnumerable<KeyValuePair<string, string?>> dict)
        {
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString("");

            foreach (var kv in dict)
            {
                nameValueCollection.Add(kv.Key, kv.Value);
            }

            return nameValueCollection;
        }

        /// <summary>
        /// ToJoinedString
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="seprator"></param>
        /// <returns></returns>

        [return: NotNullIfNotNull("ts")]
        public static string? ToJoinedString(this IEnumerable? ts, string seprator, string? nullReplacement = null)
        {
            if (ts == null)
            {
                return null;
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (object obj in ts)
            {
                if (obj != null)
                {
                    stringBuilder.Append(obj.ToString());
                    stringBuilder.Append(seprator);
                }
                else if (nullReplacement != null)
                {
                    stringBuilder.Append(nullReplacement);
                    stringBuilder.Append(seprator);
                }
            }

            if (stringBuilder.Length != 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            return stringBuilder.ToString();
        }

        public static bool TryToGuids(this string idsString, string seprator, out IList<Guid> guids)
        {
            //string[] ids = idsString.Split(seprator, StringSplitOptions.RemoveEmptyEntries);
            string[] ids = idsString.Split(new string[] { seprator }, StringSplitOptions.RemoveEmptyEntries);

            guids = new List<Guid>(ids.Length);

            foreach (string id in ids)
            {
                if (Guid.TryParse(id, out Guid guid))
                {
                    guids.Add(guid);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}