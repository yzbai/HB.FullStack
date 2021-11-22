using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

using HB.FullStack.Common.Api;
using System.IO;
using HB.FullStack.Common;
using System.Threading;

namespace System.Net.Http
{
    public static class HttpClientApiExtensions
    {
        private static readonly Type _emptyResponseType = typeof(EmptyResponse);

        private static readonly Version _version20 = new Version(2, 0);

        public static async Task<TResponse?> GetResponseAsync<TResponse>(this HttpClient httpClient, ApiRequest request, CancellationToken cancellationToken) where TResponse : class
        {
            //HttpClient不再 在接受response后主动dispose request content。 所以要主动用using dispose Request message，requestMessage dispose会dispose掉content
            using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

            if (request is IUploadRequest fileUpdateRequest)
            {
                requestMessage.Content = BuildMultipartContent(fileUpdateRequest);
            }

            using HttpResponseMessage responseMessage = await httpClient.GetResponseCoreAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);

            if (typeof(TResponse) == _emptyResponseType)
            {
                return (TResponse)(object)EmptyResponse.Value;
            }
            else
            {
                (bool success, TResponse? response) = await responseMessage.TryDeserializeJsonAsync<TResponse>().ConfigureAwait(false);

                if (!success)
                {
#if NET6_0_OR_GREATER
                    string responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#elif NETSTANDARD2_1 || NETSTANDARD2_0
                    string responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif

                    throw ApiExceptions.HttpResponseDeserializeError(request, responseString);
                }

                return response;
            }
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient httpClient, ApiRequest request, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage requestMessage = request.ToHttpRequestMessage();

            //这里不Dispose, Dipose返回的Stream的时候，会通过WrappedStream dispose这个message的
            HttpResponseMessage responseMessage = await httpClient.GetResponseCoreAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);

#if NET6_0_OR_GREATER

            return new WrappedStream(await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), responseMessage);
#elif NETSTANDARD2_1 || NETSTANDARD2_0
            return new WrappedStream(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false), responseMessage);
#endif
        }

        private static async Task<HttpResponseMessage> GetResponseCoreAsync(this HttpClient httpClient, HttpRequestMessage requestMessage, ApiRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            }

            //https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync?view=netstandard-2.1
            catch (InvalidOperationException ex)
            {
                throw ApiExceptions.RequestAlreadyUsed(request: request, innerException: ex);
            }
            catch (TaskCanceledException ex)
            {
                throw ApiExceptions.RequestTimeout(request: request, innerException: ex);
            }
            catch (HttpRequestException ex)
            {
                throw ApiExceptions.RequestUnderlyingIssue(request: request, innerException: ex);
            }
        }

        private static HttpRequestMessage ToHttpRequestMessage(this ApiRequest request)
        {
            HttpMethod httpMethod = request.HttpMethod;

            if (request.NeedHttpMethodOveride && (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Delete))
            {
                request.Headers["X-HTTP-Method-Override"] = httpMethod.Method;
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, request.GetUrl())
            {
                Version = _version20
            };

            foreach (var kv in request.Headers)
            {
                httpRequest.Headers.Add(kv.Key, kv.Value);
            }

            //Get的参数也放到body中去
            httpRequest.Content = new StringContent(SerializeUtil.ToJson(request), Encoding.UTF8, "application/json");

            return httpRequest;
        }

        [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "关于Dispose：MultipartFormDataContent Dipose的时候，会把子content Dipose掉。 而HttpRequestMessage Dispose的时候，会把他的Content Dispose掉")]
        private static MultipartFormDataContent BuildMultipartContent(IUploadRequest fileRequest)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            string httpContentName = fileRequest.HttpContentName;
            byte[] file = fileRequest.GetFile();
            string fileName = fileRequest.FileName;

            ByteArrayContent byteArrayContent = new ByteArrayContent(file);
            content.Add(byteArrayContent, httpContentName, fileName);

            content.Add(new StringContent(SerializeUtil.ToJson(fileRequest), Encoding.UTF8, "application/json"));

            return content;
        }

        public static async Task ThrowIfNotSuccessedAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            //TODO: 可以处理404等ProblemDetails的返回
            (_, ErrorCode? errorCode) = await responseMessage.TryDeserializeJsonAsync<ErrorCode>().ConfigureAwait(false);

            responseMessage.Dispose();

            if (errorCode == null)
            {
                string responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw ApiExceptions.ServerUnkownError(response: responseString);
            }
            else
            {
                throw ApiExceptions.ServerReturnError(errorCode);
            }
        }
    }
}