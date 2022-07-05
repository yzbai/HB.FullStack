﻿using System;
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
        /// <param name="urlStr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string AddQuerys(string? urlStr, IDictionary<string, string?> parameters)
        {
            //不能用UriBuilder，会将相对uri转为绝对uri

            if (urlStr == null)
            {
                string? query = AddQuerysCore("", parameters);

                return query ?? "";
            }
            else
            {
                int index = urlStr.IndexOf("?", StringComparison.Ordinal);

                string oldQuery = index > 0 ? urlStr.Substring(index + 1) : "";

                string? query = AddQuerysCore(oldQuery, parameters);

                if (index > 0)
                {
#if NET5_0_OR_GREATER
                    return string.Concat(urlStr.AsSpan(0, index + 1), query);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
                    return urlStr.Substring(0, index + 1) + query;
#endif
                }
                else
                {
                    return urlStr + "?" + query;
                }
            }

            static string? AddQuerysCore(string oldQuery, IDictionary<string, string?> parameters)
            {
                NameValueCollection queries = HttpUtility.ParseQueryString(oldQuery);

                foreach (var kv in parameters)
                {
                    queries[kv.Key] = kv.Value;
                }

                return queries.ToString();
            }
        }
    }
}