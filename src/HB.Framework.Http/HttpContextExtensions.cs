using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public static IDictionary<string, string> GetParametersFromRequest(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (httpContext.Request == null)
            {
                throw new NullReferenceException(nameof(httpContext.Request));
            }

            IDictionary<string, string> parameters = new Dictionary<string, string>();

            // Read from Query
            if (httpContext.Request.Query != null)
            {
                foreach (KeyValuePair<string, StringValues> item in httpContext.Request.Query)
                {
                    parameters.Add(item.Key, item.Value.ToString());
                }
            }

            // Read from Form
            if (httpContext.Request.HasFormContentType)
            {
                foreach (KeyValuePair<string, StringValues> item in httpContext.Request.Form)
                {
                    parameters.Add(item.Key, item.Value.ToString());
                }
            }

            //Read from Cookie
            if (httpContext.Request.Cookies != null)
            {
                foreach (KeyValuePair<string, string> item in httpContext.Request.Cookies)
                {
                    parameters.Add(item.Key, item.Value.ToString());
                }
            }

            return parameters;
        }

        public static string GetValueFromRequest(this HttpContext httpContext, string key, bool includeCookie = true)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (httpContext.Request == null)
            {
                throw new NullReferenceException(nameof(httpContext.Request));
            }

            StringValues value = StringValues.Empty;

            HttpRequest Request = httpContext.Request;

            //QueryString
            if (StringValues.IsNullOrEmpty(value) && Request != null && Request.Query != null)
            {
                value = Request.Query[key];
            }

            //Request的Body
            if (StringValues.IsNullOrEmpty(value) && Request != null && Request.HasFormContentType)
            {
                value = Request.Form[key];
            }

            if (includeCookie)
            {
                //Request Cookie
                if (StringValues.IsNullOrEmpty(value) && Request != null && Request.Cookies != null)
                {
                    value = Request.Cookies[key];
                }
            }

            return value.ToString();
        }

        public static string MaketoHttpsRawUri(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (httpContext.Request == null)
            {
                throw new NullReferenceException(nameof(httpContext.Request));
            }

            return httpContext.Request.GetDisplayUrl().Replace(httpContext.Request.Scheme, "https");
        }

        public static string GetIpAddress(this HttpContext httpContext)
        {
            string ip = httpContext.GetHeaderValueAs<string>("X-Forwarded-For");

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress.ToString();
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = httpContext.GetHeaderValueAs<string>("REMOTE_ADDR");
            }

            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }

        public static T GetHeaderValueAs<T>(this HttpContext httpContext, string headerName)
        {
            StringValues values;

            if (httpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        //TODO: add requset info from cookie, form ,query etc.
        public static string RequestDebugInformation(this HttpContext httpContext)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine();

            return builder.ToString();
        }
    }
}
