using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using System.IO;

using System.Threading;

namespace System.Net.Http
{
    public static class HttpClientApiExtensions
    {
        private static readonly Type _emptyResponseType = typeof(EmptyResponse);

        public static async Task<TResponse?> GetAsync<TResponse>(this HttpClient httpClient, ApiRequest request, HttpRequestMessageBuilder httpRequestMessageBuilder, CancellationToken cancellationToken) where TResponse : class
        {
            //NOTICE:HttpClient不再 在接受response后主动dispose request content。
            //所以要主动用using dispose Request message，requestMessage dispose会dispose掉content
            using HttpRequestMessage requestMessage = httpRequestMessageBuilder.Build(request);

            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

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

        public static async Task<Stream> GetStreamAsync(this HttpClient httpClient, ApiRequest request, HttpRequestMessageBuilder httpRequestMessageBuilder, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage requestMessage = httpRequestMessageBuilder.Build(request);

            //这里不Dispose, Dipose返回的Stream的时候，会通过WrappedStream dispose这个message的
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, request, cancellationToken).ConfigureAwait(false);

            await ThrowIfNotSuccessedAsync(responseMessage).ConfigureAwait(false);

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

        public static async Task ThrowIfNotSuccessedAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            //TODO: 可以处理404等ProblemDetails的返回
            (bool success, ErrorCode? errorCode) = await responseMessage.TryDeserializeJsonAsync<ErrorCode>().ConfigureAwait(false);

            //responseMessage.Dispose();

            if (success && errorCode != null)
            {
                throw ApiExceptions.ServerReturnError(errorCode);
            }
            else
            {
                string? responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw ApiExceptions.ServerUnkownError(response: responseString);
            }
        }
    }
}