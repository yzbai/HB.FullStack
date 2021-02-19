using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace System
{
    public static class UrlUtil
    {
        public static string AddQuerys(string uri, IDictionary<string, string?> parameters)
        {
            //不能用UriBuilder，会将相对uri转为绝对uri
            //UriBuilder uriBuilder = new UriBuilder(uri);

            int index = uri.IndexOf('?');

            string oldQuery = index > 0 ? uri.Substring(index + 1) : "";

            NameValueCollection queries = HttpUtility.ParseQueryString(oldQuery);

            foreach (var kv in parameters)
            {
                queries[kv.Key] = kv.Value;
            }

            if(index > 0)
            {
                return uri.Substring(0, index + 1) + queries.ToString();
            }
            else
            {
                return uri + "?" + queries.ToString();
            }
        }
    }
}