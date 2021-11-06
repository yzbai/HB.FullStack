using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public interface IApiClient
    {
        [SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "<Pending>")]
        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;
        [SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "<Pending>")]
        event AsyncEventHandler<object, ApiEventArgs> Responsed;

        JwtEndpointSetting GetDefaultJwtEndpointSetting();

        Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken);

        Task<Stream> GetStreamAsync(ApiRequest request) => GetStreamAsync(request, CancellationToken.None);

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class => GetAsync<TResponse>(request, CancellationToken.None);  

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken) => GetAsync<EmptyResponse>(request, cancellationToken);

        Task SendAsync(ApiRequest request) => SendAsync(request, CancellationToken.None);

    }
}