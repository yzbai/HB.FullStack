using System;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public interface IApiClient
    {
        IPreferenceProvider SignInReceiptProvider { get; }

        ResEndpoint? SignInReceiptResEndpoint { get; }

        event Func<ApiRequest, ApiEventArgs, Task>? Requesting;

        event Func<object?, ApiEventArgs, Task>? Responsed;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken);

        Task SendAsync(ApiRequest request);
    }
}