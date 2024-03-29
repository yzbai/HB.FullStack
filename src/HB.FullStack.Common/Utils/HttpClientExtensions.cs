﻿using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 尝试解析Json,可以解析返回（true,data),data 可能为null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static async Task<(bool, T?)> TryDeserializeJsonContentAsync<T>(this HttpResponseMessage responseMessage) where T : class
        {
            string? mediaType = responseMessage.Content.Headers.ContentType?.MediaType;

            if ("application/json" != mediaType && "application/problem+json" != mediaType)
            {
                return (false, null);
            }

            string jsonString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                //T? data = SerializeUtil.FromJson<T>(jsonString);
                if (SerializeUtil.TryFromJsonWithCollectionCheck(jsonString, out T? data))
                {
                    return (true, data);
                }
                else
                {
                    return (false, null);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Globals.Logger?.LogHttpResponseDeSerializeJsonError(jsonString, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase, responseMessage?.RequestMessage?.RequestUri, ex);
                return (false, null);
            }
        }

        public static bool IsNoInternet(this HttpStatusCode statusCode)
        {
            int httpCode = (int)statusCode;

            return httpCode >= 500 && httpCode < 600;
        }
    }
}
