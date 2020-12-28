﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace System
{
    public static class HttpContextExtensions
    {
        public static IDictionary<string, string> GetParameters(this HttpRequest request)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();

            // Read from Query
            if (request.Query != null)
            {
                foreach (KeyValuePair<string, StringValues> item in request.Query)
                {
                    parameters.Add(item.Key, item.Value.ToString());
                }
            }

            // Read from Form
            if (request.HasFormContentType)
            {
                foreach (KeyValuePair<string, StringValues> item in request.Form)
                {
                    parameters.Add(item.Key, item.Value.ToString());
                }
            }

            //Read from Cookie
            if (request.Cookies != null)
            {
                foreach (KeyValuePair<string, string> item in request.Cookies)
                {
                    parameters.Add(item.Key, item.Value.ToString(GlobalSettings.Culture));
                }
            }

            return parameters;
        }

        //[Obsolete("无法处理JsonHttpContent")]
        public static string GetValue(this HttpRequest request, string key, bool includeCookie = false)
        {
            ThrowIf.Empty(key, nameof(key));

            StringValues value = StringValues.Empty;

            //QueryString
            if (StringValues.IsNullOrEmpty(value) && request != null && request.Query != null)
            {
                value = request.Query[key];
            }

            //Request的Body
            if (StringValues.IsNullOrEmpty(value) && request != null && request.HasFormContentType)
            {
                value = request.Form[key];
            }

            if (includeCookie)
            {
                //Request Cookie
                if (StringValues.IsNullOrEmpty(value) && request != null && request.Cookies != null)
                {
                    value = request.Cookies[key];
                }
            }

            return value.ToString();
        }

        public static Uri MakeToHttpsRawUri(this HttpRequest request)
        {
            string url = request.GetEncodedUrl().Replace(request.Scheme, "https", GlobalSettings.Comparison);

            return new Uri(url);
        }

        public static string GetIpAddress(this HttpContext httpContext)
        {
            string? ip = httpContext.Request.GetHeaderValueAs<string>("X-Forwarded-For");

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Request.GetHeaderValueAs<string>("X-Real-IP");
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                try
                {
                    ip = httpContext.Connection.RemoteIpAddress.ToString();
                }
                catch
                {
                    ip = null;
                }
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Request.GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }

        /// <summary>
        /// GetHeaderValueAs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public static T? GetHeaderValueAs<T>(this HttpRequest request, string headerName) where T : class
        {
            try
            {
                StringValues values;

                if (request.Headers?.TryGetValue(headerName, out values) ?? false)
                {
                    string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                    if (!string.IsNullOrEmpty(rawValues))
                        return (T)Convert.ChangeType(values.ToString(), typeof(T), GlobalSettings.Culture);
                }
                return default;
            }
            catch
            {
                return default;
            }
        }

    }
}
