using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<T?> DeSerializeJsonAsync<T>(this HttpResponseMessage responseMessage) where T : class
        {
            string? mediaType = responseMessage.Content.Headers.ContentType?.MediaType;

            if ("application/json" != mediaType && "application/problem+json" != mediaType)
            {
                return null;
            }

            string jsonString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                T? data = SerializeUtil.FromJson<T>(jsonString);

                return data;
            }
            catch
            {
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
