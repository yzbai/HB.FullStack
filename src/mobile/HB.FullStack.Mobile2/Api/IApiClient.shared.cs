using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.XamarinForms.Api
{
    public interface IApiClient
    {
#pragma warning disable CA1003 // Use generic event handler instances

        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;

        event AsyncEventHandler<object, ApiEventArgs> Responsed;

#pragma warning restore CA1003 // Use generic event handler instances

        /// <exception cref="ApiException"></exception>
        Task AddAsync<T>(AddRequest<T> request) where T : ApiResource;

        /// <exception cref="ApiException"></exception>
        Task UpdateAsync<T>(UpdateRequest<T> request) where T : ApiResource;

        /// <exception cref="ApiException"></exception>
        Task DeleteAsync<T>(DeleteRequest<T> request) where T : ApiResource;

        /// <exception cref="ApiException"></exception>
        Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : ApiResource;

        Task<T?> GetFirstOrDefaultAsync<T>(ApiRequest<T> request) where T : ApiResource;
        JwtEndpointSetting GetDefaultJwtEndpointSetting();
        Task<Stream> GetStreamAsync(ApiRequest request);
    }
}