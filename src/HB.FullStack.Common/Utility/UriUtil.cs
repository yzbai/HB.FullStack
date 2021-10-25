using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace System
{
    public static class UriUtil
    {
        /// <summary>
        /// 在字符串末尾添加参数
        /// </summary>
        /// <param name="str"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string AddQuerys(string str, IDictionary<string, string?> parameters)
        {
            //不能用UriBuilder，会将相对uri转为绝对uri
            //UriBuilder uriBuilder = new UriBuilder(uri);

            int index = str.IndexOf('?', StringComparison.InvariantCulture);

            string oldQuery = index > 0 ? str[(index + 1)..] : "";

            NameValueCollection queries = HttpUtility.ParseQueryString(oldQuery);

            foreach (var kv in parameters)
            {
                queries[kv.Key] = kv.Value;
            }

            if (index > 0)
            {
                return str[..(index + 1)] + queries.ToString();
            }
            else
            {
                return str + "?" + queries.ToString();
            }
        }
    }
}