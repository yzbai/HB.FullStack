using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 尝试解析Json，不成功返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static async Task<T?> TryDeserializeJsonAsync<T>(this HttpResponseMessage responseMessage) where T : class
        {
            string? mediaType = responseMessage.Content.Headers.ContentType?.MediaType;

            if ("application/json" != mediaType && "application/problem+json" != mediaType)
            {
                return null;
            }

            string jsonString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                //T? data = SerializeUtil.FromJson<T>(jsonString);
                if (SerializeUtil.TryFromJsonWithCollectionCheck<T>(jsonString, out T? data))
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                GlobalSettings.Logger?.LogHttpResponseDeSerializeJsonError(jsonString, ((int)responseMessage.StatusCode), responseMessage.ReasonPhrase, responseMessage.RequestMessage.RequestUri, ex);
                return null;
            }
        }

        public static bool IsNoInternet(this HttpStatusCode statusCode)
        {
            int httpCode = (int)statusCode;

            return httpCode >= 500 && httpCode < 600;
        }
    }
}
