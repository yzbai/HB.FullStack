using HB.Framework.Client.Properties;
using HB.Framework.Common.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public static class ApiExtensions
    {
        /// <exception cref="HB.Framework.Client.ClientException"></exception>
        public static async Task<ApiResponse> GetApiResponse(this ApiRequest request, Type? dataType, HttpClient httpClient, bool needHttpMethodOveride)
        {
            using HttpRequestMessage httpRequest = request.ToHttpRequestMessage(needHttpMethodOveride);

            using HttpResponseMessage httpResponse = await httpRequest.GetHttpResponseMessage(httpClient).ConfigureAwait(false);

            return await httpResponse.ToApiResponseAsync(dataType).ConfigureAwait(false);
        }

        /// <exception cref="HB.Framework.Client.ClientException"></exception>
        public static async Task<HttpResponseMessage> GetHttpResponseMessage(this HttpRequestMessage httpRequestMessage, HttpClient httpClient)
        {
            try
            {
                return await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ClientException($"ApiClient.GetResponseActual Error", ex);
            }
        }

        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public static HttpRequestMessage ToHttpRequestMessage(this ApiRequest request, bool needHttpMethodOveride)
        {
            HttpMethod httpMethod = request.GetHttpMethod();

            if (needHttpMethodOveride && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                request.AddHeader("X-HTTP-Method-Override", httpMethod.Method);
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, ToUrl(request));

            if (request.GetHttpMethod() != HttpMethod.Get)
            {
                httpRequest.Content = new FormUrlEncodedContent(request.GetParameters());
            }

            request.GetHeaders().ForEach(kv => httpRequest.Headers.Add(kv.Key, kv.Value));

            return httpRequest;
        }

#pragma warning disable CA1055 // Uri return values should not be strings
        public static string ToUrl(this ApiRequest request)
#pragma warning restore CA1055 // Uri return values should not be strings
        {
            StringBuilder requestUrlBuilder = new StringBuilder();

            if (!request.GetApiVersion().IsNullOrEmpty())
            {
                requestUrlBuilder.Append(request.GetApiVersion());
            }

            if (!request.GetResourceName().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetResourceName());
            }

            if (!request.GetCondition().IsNullOrEmpty())
            {
                requestUrlBuilder.Append("/");
                requestUrlBuilder.Append(request.GetCondition());
            }

            if (request.GetHttpMethod() == HttpMethod.Get)
            {
                string query = request.GetParameters().ToHttpValueCollection().ToString();

                if (!query.IsNullOrEmpty())
                {
                    requestUrlBuilder.Append("?");
                    requestUrlBuilder.Append(query);
                }
            }

            return requestUrlBuilder.ToString();
        }

        /// <exception cref="System.Text.Json.JsonException">Ignore.</exception>
        public static async Task<ApiResponse> ToApiResponseAsync(this HttpResponseMessage httpResponse, Type? dataType)
        {
            Stream responseStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode)
            {
                object? data = dataType == null ? null : await SerializeUtil.FromJsonAsync(dataType, responseStream).ConfigureAwait(false);

                return new ApiResponse(data as ApiResponseData, (int)httpResponse.StatusCode);
            }
            else
            {
                string mediaType = httpResponse.Content.Headers.ContentType.MediaType;

                if (mediaType == "application/problem+json" || mediaType == "application/json")
                {
                    ApiErrorResponse errorResponse = await SerializeUtil.FromJsonAsync<ApiErrorResponse>(responseStream).ConfigureAwait(false);

                    return new ApiResponse((int)httpResponse.StatusCode, errorResponse.Message, errorResponse.Code);
                }
                else
                {
                    return new ApiResponse((int)httpResponse.StatusCode, Resources.InternalServerErrorMessage, ApiError.ApiInternalError);
                }
            }
        }
    }
}
