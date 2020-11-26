using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace HB.FullStack.Common.Utility
{
    public static class UrlUtil
    {
        public static Uri AddQuery(this Uri uri, string key, string value)
        {
            return uri.AddQuerys(new Dictionary<string, string> { { key, value } });
        }

        public static Uri AddQuerys(this Uri uri, IDictionary<string, string> parameters)
        {
            UriBuilder uriBuilder = new UriBuilder(uri);

            NameValueCollection queries = HttpUtility.ParseQueryString(uriBuilder.Query);

            parameters.ForEach(kv => { queries[kv.Key] = kv.Value; });

            uriBuilder.Query = queries.ToString();

            return uriBuilder.Uri;
        }
    }
}