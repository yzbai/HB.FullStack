using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

using HB.FullStack.Common.Api;
using System.IO;

namespace System.Net.Http
{
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class EmptyResponse
    {
        public static EmptyResponse Value { get; }

        static EmptyResponse()
        {
            Value = new EmptyResponse();
        }

        private EmptyResponse() { }
    }

    public static class HttpClientApiExtensions
    {
        private static readonly Type _emptyResponseType = typeof(EmptyResponse);

        /// <exception cref="System.ApiException"></exception>
        public static async Task<TResponse?> SendAsync<TResource, TResponse>(this HttpClient httpClient, ApiRequest<TResource> request) where TResource : ModelObject where TResponse : class
        {
            using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

            if (request is FileUpdateRequest<TResource> fileUpdateRequest)
            {
                requestMessage.Content = BuildMultipartContent(fileUpdateRequest);
            }

            using HttpResponseMessage responseMessage = await httpClient.SendCoreAsync(requestMessage, request).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);

            if (typeof(TResponse) == _emptyResponseType)
            {
                return (TResponse)(object)EmptyResponse.Value;
            }
            else
            {
                TResponse? response = await responseMessage.DeSerializeJsonAsync<TResponse>().ConfigureAwait(false);

                return response;
            }
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient httpClient, ApiRequest request)
        {
            using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

            //这里不Dispose
            HttpResponseMessage responseMessage = await httpClient.SendCoreAsync(requestMessage, request).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);

            return new WrappedStream(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false), responseMessage);
        }

        /// <exception cref="ApiException"></exception>
        private static async Task<HttpResponseMessage> SendCoreAsync(this HttpClient httpClient, HttpRequestMessage requestMessage, ApiRequest request)
        {
            try
            {
                return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                // Handle timeout.
                throw ApiExceptions.ClientTimeout(request: request, innerException: ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is SocketException)
            {
                throw ApiExceptions.ApiNotAvailable(request: request, innerException: ex);
            }
            catch (TaskCanceledException ex)
            {
                // Handle cancellation.
                throw ApiExceptions.RequestCanceled(request: request, innerException: ex);
            }
            //TODO: when using .net 5
            //catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            //{
            //    // Handle 404
            //    Console.WriteLine("Not found: " + ex.Message);
            //}
            catch (OperationCanceledException ex)
            {
                throw ApiExceptions.ApiNotAvailable(request, ex);
            }
            catch (Exception ex)
            {
                throw ApiExceptions.ClientUnkownError(request: request, innerException: ex);
            }
        }

        private static HttpRequestMessage ToHttpRequestMessage(this ApiRequest request)
        {
            HttpMethod httpMethod = request.GetHttpMethod();

            if (request.GetNeedHttpMethodOveride() && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                request.SetHeader("X-HTTP-Method-Override", httpMethod.Method);
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, request.GetUrl());

            foreach (var kv in request.GetHeaders())
            {
                httpRequest.Headers.Add(kv.Key, kv.Value);
            }

            //Get的参数也放到body中去
            httpRequest.Content = new StringContent(SerializeUtil.ToJson(request), Encoding.UTF8, "application/json");

            return httpRequest;
        }

        [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private static MultipartFormDataContent BuildMultipartContent<T>(FileUpdateRequest<T> fileRequest) where T : ModelObject
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            string byteArrayContentName = fileRequest.GetBytesPropertyName();
            IEnumerable<byte[]> bytess = fileRequest.GetBytess();
            IEnumerable<string> fileNames = fileRequest.GetFileNames();

            int num = 0;

            foreach (string fileName in fileNames)
            {
                byte[] bytes = bytess.ElementAt(num++);

                ByteArrayContent byteArrayContent = new ByteArrayContent(bytes);
                content.Add(byteArrayContent, byteArrayContentName, fileName);
            }

            //request.GetParameters().ForEach(kv =>
            //{
            //    content.Add(new StringContent(kv.Value), kv.Key);
            //});

            content.Add(new StringContent(SerializeUtil.ToJson(fileRequest), Encoding.UTF8, "application/json"));

            return content;
        }

        /// <exception cref="ApiException"></exception>
        public static async Task ThrowIfNotSuccessedAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            //TODO: 可以处理404等ProblemDetails的返回
            ErrorCode? errorCode = await responseMessage.DeSerializeJsonAsync<ErrorCode>().ConfigureAwait(false);

            responseMessage.Dispose();

            if (errorCode == null)
            {
                throw ApiExceptions.ServerUnkownError(response: responseMessage);
            }
            else
            {
                throw ApiExceptions.ServerReturnError(errorCode);
            }

        }
    }
}