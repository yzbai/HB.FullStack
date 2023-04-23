/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Client.Abstractions;

namespace HB.FullStack.Client.ApiClient
{
    public interface IApiClient
    {
        event Func<ApiRequest, ApiEventArgs, Task>? Requesting;

        event Func<object?, ApiEventArgs, Task>? Responsed;


        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken);

        Task SendAsync(ApiRequest request);
        
        internal ITokenPreferences TokenPreferences { get; }

        internal ApiClientOptions ApiClientOptions { get; }
    }
}