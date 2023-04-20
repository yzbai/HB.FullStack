using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components.KVManager
{
    public interface IKVManager
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T? value, TimeSpan? aliveTime);

        Task DeleteAsync(string key);
    }
}