using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.Properties;

namespace HB.FullStack.Client.Api
{
    public static class ApiExtensions
    {
        private static readonly string[] _jsonContentTypes = new string[] { "application/json", "application/problem+json" };

        public static async Task<ApiResponse<T>> GetResponseAsync<T>(this ApiRequest request, HttpClient httpClient) where T : class
        {
            try
            {
                using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

                using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                return await responseMessage.ToApiResponseAsync<T>().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiError, $"ApiRequestUtils.GetResponse {request.GetResourceName()}", ex);
            }
        }

        public static async Task<ApiResponse> GetResponseAsync(this ApiRequest request, HttpClient httpClient)
        {
            try
            {
                using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

                using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                return await responseMessage.ToApiResponseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApiException(ErrorCode.ApiError, $"ApiRequestUtils.GetResponse {request.GetResourceName()}", ex);
            }
        }

        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public static HttpRequestMessage ToHttpRequestMessage(this ApiRequest request)
        {
            HttpMethod httpMethod = request.GetHttpMethod();

            if (request.GetNeedHttpMethodOveride() && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                request.SetHeader("X-HTTP-Method-Override", httpMethod.Method);
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, BuildUrl(request));

            //Get的参数也放到body中去
            //if (request.GetHttpMethod() != HttpMethod.Get)
            //{
            if (request is BufferedFileApiRequest bufferedRequest)
            {
                MultipartFormDataContent content = new MultipartFormDataContent();

#pragma warning disable CA2000 // Dispose objects before losing scope // using HttpRequestMessage 会自动dispose他的content
                ByteArrayContent byteArrayContent = new ByteArrayContent(bufferedRequest.GetBytes());
#pragma warning restore CA2000 // Dispose objects before losing scope

                content.Add(byteArrayContent, bufferedRequest.GetBytesPropertyName(), bufferedRequest.GetFileName());

                //request.GetParameters().ForEach(kv =>
                //{
                //    content.Add(new StringContent(kv.Value), kv.Key);
                //});

#pragma warning disable CA2000 // Dispose objects before losing scope // HttpRequestMessage dispose时候，会dispose他的content
                content.Add(new StringContent(SerializeUtil.ToJson(request), Encoding.UTF8, "application/json"));
#pragma warning restore CA2000 // Dispose objects before losing scope

                httpRequest.Content = content;
            }
            else
            {
                //TODO: .net 5以后，使用JsonContent
                httpRequest.Content = new StringContent(SerializeUtil.ToJson(request), Encoding.UTF8, "application/json");
            }
            //}

            request.GetHeaders().ForEach(kv => httpRequest.Headers.Add(kv.Key, kv.Value));

            return httpRequest;
        }

        private static string BuildUrl(ApiRequest request)
        {
            StringBuilder requestUrlBuilder = new StringBuilder();

            if (!request.GetApiVersion().IsNullOrEmpty())
            {
                requestUrlBuilder.Append(request.GetApiVersion());
            }

            if (!request.GetResourceName().IsNullOrEmpty())
            {
                requestUrlBuilder.Append('/');
                requestUrlBuilder.Append(request.GetResourceName());
            }

            if (!request.GetCondition().IsNullOrEmpty())
            {
                requestUrlBuilder.Append('/');
                requestUrlBuilder.Append(request.GetCondition());
            }

            //添加噪音
            IDictionary<string, string?> parameters = new Dictionary<string, string?>();
            parameters.Add(ClientNames.RandomStr, ApiRequest.GetRandomStr());
            parameters.Add(ClientNames.Timestamp, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture));

            //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
            parameters.Add(ClientNames.DeviceId, request.DeviceId);

            string query = parameters.ToHttpValueCollection().ToString();
            requestUrlBuilder.Append('?');
            requestUrlBuilder.Append(query);

            //放到Body中去
            //if (request.GetHttpMethod() == HttpMethod.Get)
            //{
            //    string query = request.GetParameters().ToHttpValueCollection().ToString();

            //    if (!query.IsNullOrEmpty())
            //    {
            //        requestUrlBuilder.Append('?');
            //        requestUrlBuilder.Append(query);
            //    }
            //}

            return requestUrlBuilder.ToString();
        }

        /// <exception cref="System.Text.Json.JsonException">Ignore.</exception>
        public static async Task<ApiResponse<T>> ToApiResponseAsync<T>(this HttpResponseMessage httpResponse) where T : class
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                T? data = await httpResponse.DeSerializeJsonAsync<T>().ConfigureAwait(false);
                return new ApiResponse<T>(data, (int)httpResponse.StatusCode);
            }

            ApiError? apiError = await httpResponse.DeSerializeJsonAsync<ApiError>().ConfigureAwait(false);

            if (apiError == null)
            {
                return new ApiResponse<T>((int)httpResponse.StatusCode, Resources.InternalServerErrorMessage, ErrorCode.ApiError);
            }

            return new ApiResponse<T>((int)httpResponse.StatusCode, apiError.Message, apiError.Code);
        }

        public static async Task<ApiResponse> ToApiResponseAsync(this HttpResponseMessage httpResponse)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                return new ApiResponse((int)httpResponse.StatusCode);
            }

            ApiError? apiError = await httpResponse.DeSerializeJsonAsync<ApiError>().ConfigureAwait(false);

            if (apiError == null)
            {
                return new ApiResponse((int)httpResponse.StatusCode, Resources.InternalServerErrorMessage, ErrorCode.ApiError);
            }

            return new ApiResponse((int)httpResponse.StatusCode, apiError.Message, apiError.Code);
        }

        public static async Task<T?> DeSerializeJsonAsync<T>(this HttpResponseMessage responseMessage) where T : class
        {
            if (typeof(T) == typeof(object))
            {
                return null;
            }

            string? mediaType = responseMessage.Content.Headers.ContentType?.MediaType;

            if (!_jsonContentTypes.Contains(mediaType))
            {
                return null;
            }

            //using MemoryStream memoryStream = new MemoryStream();
            //await responseMessage.Content.CopyToAsync(memoryStream).ConfigureAwait(false);

            ////Stream responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            //T? data = await SerializeUtil.FromJsonAsync<T>(memoryStream).ConfigureAwait(false);

            string jsonString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            T? data = SerializeUtil.FromJson<T>(jsonString);

            return data;
        }
    }
}