using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Client.Api
{
    public interface IApiClient
    {
        Task AddAsync<T>(ApiRequest<T> request) where T : Resource;
        Task DeleteAsync<T>(ApiRequest<T> request) where T : Resource;
        Task<IEnumerable<T>> GetAsync<T>(ApiRequest<T> request) where T : Resource;
        Task<T?> GetSingleAsync<T>(ApiRequest<T> request) where T : Resource;
        Task UpdateAsync<T>(ApiRequest<T> request) where T : Resource;
    }
}