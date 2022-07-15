using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.Utils;

using Microsoft.Net.Http.Headers;

namespace System.Net.Http
{
    public static class HttpClientApiExtensions
    {
        private static readonly Type _emptyResourceType = typeof(EmptyApiResource);

        public static async Task<TResource?> GetAsync<TResource>(this HttpClient httpClient, ApiRequest request, CancellationToken cancellationToken) where TResource : class
        {
            //NOTICE:HttpClient不再 在接受response后主动dispose request content。
            //所以要主动用using dispose Request message，requestMessage dispose会dispose掉content
            using HttpRequestMessage requestMessage = request.GetHttpRequestBuilder().Build(request);

            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage, request.GetHttpRequestBuilder().EndpointSettings.Challenge).ConfigureAwait(false);

            if (typeof(TResource) == _emptyResourceType)
            {
                return (TResource)(object)EmptyApiResource.Value;
            }
            else
            {
                (bool success, TResource? response) = await responseMessage.TryDeserializeJsonContentAsync<TResource>().ConfigureAwait(false);

                if (!success)
                {
#if NET5_0_OR_GREATER
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
            using HttpRequestMessage requestMessage = request.GetHttpRequestBuilder().Build(request);

            //这里不Dispose, Dipose返回的Stream的时候，会通过WrappedStream dispose这个message的
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage, request.GetHttpRequestBuilder().EndpointSettings.Challenge).ConfigureAwait(false);

#if NET5_0_OR_GREATER
            return new WrappedStream(await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), responseMessage);
#elif NETSTANDARD2_1 || NETSTANDARD2_0
            return new WrappedStream(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false), responseMessage);
#endif
        }

        private static async Task<HttpResponseMessage> SendAsync(this HttpClient httpClient, HttpRequestMessage requestMessage, ApiRequest request, CancellationToken cancellationToken)
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

        public static async Task ThrowIfNotSuccessedAsync(HttpResponseMessage responseMessage, string challenge)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            //开始处理错误

            //401, 解析Header
            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (responseMessage.Headers.TryGetValues(HeaderNames.WWWAuthenticate, out IEnumerable<string>? headValues) && headValues.Count() == 1)
                {
                    string authenticate = headValues.First();

                    if (authenticate.StartsWith(challenge, StringComparison.OrdinalIgnoreCase))
                    {
                        authenticate = authenticate.Substring(challenge.Length + 1);

                        if (SerializeUtil.TryFromJson(authenticate, out ErrorCode? authErrorCode))
                        {
                            if (authErrorCode != null)
                            {
                                throw new ApiException(authErrorCode);
                            }
                        }
                    }
                }
            }

            //TODO: 可以处理404等ProblemDetails的返回
            (bool success, ErrorCode? errorCode) = await responseMessage.TryDeserializeJsonContentAsync<ErrorCode>().ConfigureAwait(false);

            //responseMessage.Dispose();

            if (success && errorCode != null)
            {
                throw new ApiException(errorCode);
            }
            else
            {
                string? responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw ApiExceptions.ServerUnkownError(response: responseString);
            }
        }
    }
}