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
        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;

        event AsyncEventHandler<object, ApiEventArgs> Responsed;

        JwtEndpointSetting GetDefaultJwtEndpointSetting();

        //Task<Stream> GetStreamAsync(ApiRequest request, CancellationToken cancellationToken);

        //Task<Stream> GetStreamAsync(ApiRequest request);

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request, CancellationToken cancellationToken) where TResponse : class;

        Task<TResponse?> GetAsync<TResponse>(ApiRequest request) where TResponse : class;

        Task SendAsync(ApiRequest request, CancellationToken cancellationToken);

        Task SendAsync(ApiRequest request);
    }
}