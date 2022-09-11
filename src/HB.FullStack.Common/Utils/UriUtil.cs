using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace System
{
    public static class UriUtil
    {
        private static IList<KeyValuePair<string, string>> NoiseQueryParameters =>
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) ),
                new KeyValuePair<string, string>(ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture))
            };

        public static string NoiseQueryString => ConvertToQueryString(NoiseQueryParameters);

        public static string AddNoiseQuery(this string? urlStr)
        {
            return AddQuerys(urlStr, NoiseQueryParameters);
        }

        public static Uri ToUri(this string? urlStr)
        {
            return new Uri(urlStr ?? string.Empty);
        }

        public static string AddQueryString(this string? urlStr, string queryString)
        {
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

        public static string AddQuery(this string? urlStr, string parameterName, string? parameterValue)
        {
            return AddQueryString(urlStr, $"{parameterName}={HttpUtility.UrlEncode(parameterValue)}");
        }

        public static string AddQuerys(this string? urlStr, IList<KeyValuePair<string, string>> parameters)
        {
            //不能用UriBuilder，会将相对uri转为绝对uri

            string queryString = ConvertToQueryString(parameters);

            return AddQueryString(urlStr, queryString);
        }

        public static string ConvertToQueryString(this IList<KeyValuePair<string, string>>? parameters)
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