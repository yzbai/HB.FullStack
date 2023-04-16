using System;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.ApiClient
{
    public interface IApiClient
    {
        event Func<ApiRequest, ApiEventArgs, Task>? Requesting;

        event Func<object?, ApiEventArgs, Task>? Responsed;
        
        IPreferenceProvider PreferenceProvider { get; }

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken);

        Task SendAsync(ApiRequest request);
    }
}