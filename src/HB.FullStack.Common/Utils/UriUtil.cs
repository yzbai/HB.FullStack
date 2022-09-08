using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace System
{
    public static class UriUtil
    {
        /// <summary>
        /// 在字符串末尾添加参数
        /// </summary>
        public static string AddQuerys(string? urlStr, IList<KeyValuePair<string, string>> parameters)
        {
            //不能用UriBuilder，会将相对uri转为绝对uri

            string queryString = ConvertToQueryString(parameters);

            if (urlStr.IsNullOrEmpty())
            {
                return queryString;
            }
            if (urlStr.Contains('?', StringComparison.Ordinal))
            {
                return urlStr + '&' + queryString;
            }
            else
            {
                return urlStr + '?' + queryString;
            }

        }

        public static string ConvertToQueryString(IList<KeyValuePair<string, string>>? parameters)
        {
            if (parameters.IsNullOrEmpty())
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in parameters)
            {
                builder.Append(kv.Key);
                builder.Append('=');
                builder.Append(HttpUtility.UrlEncode(kv.Value));
                builder.Append('&');
            }

            builder.RemoveLast();

            return builder.ToString();
        }
    }
}