using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;
using System.Globalization;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public static IDictionary<string, string> GetParameters(this HttpRequest request)
        {
            ThrowIf.Null(request, nameof(request));

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

        public static string GetValue(this HttpRequest request, string key, bool includeCookie = false)
        {
            ThrowIf.Null(request, nameof(request));
            ThrowIf.NullOrEmpty(key, nameof(key));

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
            ThrowIf.Null(request, nameof(request));

            string url = request.GetEncodedUrl().Replace(request.Scheme, "https");

            return new Uri(url);
        }

        public static string GetIpAddress(this HttpContext httpContext)
        {
            ThrowIf.Null(httpContext, nameof(httpContext));

            string ip = httpContext.Request.GetHeaderValueAs<string>("X-Forwarded-For");

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Request.GetHeaderValueAs<string>("X-Real-IP");
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress.ToString();
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Request.GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }

        public static T GetHeaderValueAs<T>(this HttpRequest request, string headerName)
        {
            ThrowIf.Null(request, nameof(request));

            StringValues values;

            if (request.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T), GlobalSettings.Culture);
            }
            return default;
        }

    }
}
