using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components.KVManager
{
    //Remark:
    //Cache的KV，需要思考污染问题；KVStore需要考虑持久性，污染问题；而这里的KV使用在客户端，只要最简单的实现
    public interface IKVManager
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T? value, TimeSpan? aliveTime);

        Task DeleteAsync(string key);
    }
}