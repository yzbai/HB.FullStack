using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Mobile.Api
{
    public interface IApiClient
    {
#pragma warning disable CA1003 // Use generic event handler instances

        event AsyncEventHandler<ApiRequest, ApiEventArgs> Requesting;

        event AsyncEventHandler<object, ApiEventArgs> Responsed;

#pragma warning restore CA1003 // Use generic event handler instances

        Task AddAsync<T>(AddRequest<T> request) where T : Resource;

        Task UpdateAsync<T>(UpdateRequest<T> request) where T : Resource;

        Task DeleteAsync<T>(DeleteRequest<T> request) where T : Resource;

        Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : Resource;

        Task<T?> GetSingleAsync<T>(ApiRequest<T> request) where T : Resource;


    }
}