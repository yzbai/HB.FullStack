using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Utils;

using Microsoft.Net.Http.Headers;

namespace System.Net.Http
{
    public static class HttpClientApiExtensions
    {
        private static readonly Type _emptyResourceType = typeof(EmptyApiResource);

        public static async Task<TResource?> GetAsync<TResource>(this HttpClient httpClient, ApiRequest request, HttpRequestMessageBuilder requestBuilder, CancellationToken cancellationToken) where TResource : class
        {
            //NOTICE:HttpClient不再 在接受response后主动dispose request content。
            //所以要主动用using dispose Request message，requestMessage dispose会dispose掉content
            using HttpRequestMessage requestMessage = requestBuilder.Build();

            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage, requestBuilder.SiteSetting.Challenge).ConfigureAwait(false);

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

                    throw CommonExceptions.ApiClientInnerError("Server返回成功，但Json解析不成功", null, new { Request = request, ResponseString = responseString });
                }

                return response;
            }
        }

        public static async Task<Stream> GetStreamAsync(this HttpClient httpClient, ApiRequest request, HttpRequestMessageBuilder requestBuilder, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage requestMessage = requestBuilder.Build();

            //这里不Dispose, Dipose返回的Stream的时候，会通过WrappedStream dispose这个message的
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage, requestBuilder.SiteSetting.Challenge).ConfigureAwait(false);

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
            catch (Exception ex)
            {
                throw CommonExceptions.ApiClientInnerError("HttpClient.SendAsync出错", ex, new { ApiRequest = request, RequestUri = requestMessage.RequestUri });
            }
        }

        public static async Task ThrowIfNotSuccessedAsync(HttpResponseMessage responseMessage, string challenge)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            //开始处理错误

            //step 1: 401, 从Header解析ErrorCode
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
                                throw CommonExceptions.ServerReturnError(authErrorCode);
                            }
                        }
                    }
                }
            }

            //TODO: 可以处理404等ProblemDetails的返回

            //step 2: 从内容处理
            (bool success, ErrorCode? errorCode) = await responseMessage.TryDeserializeJsonContentAsync<ErrorCode>().ConfigureAwait(false);

            //responseMessage.Dispose();

            if (success && errorCode != null)
            {
                throw CommonExceptions.ServerReturnError(errorCode);
            }
            else
            {
                string? responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw CommonExceptions.ServerUnkownError(responseString: responseString);
            }
        }
    }
}