using System;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public interface IApiClient
    {
        IPreferenceProvider UserTokenProvider { get; }

        ResBinding? UserTokenResBinding { get; }

        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;

        event AsyncEventHandler<object, ApiEventArgs> Responsed;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken);

        Task SendAsync(ApiRequest request);
    }
}