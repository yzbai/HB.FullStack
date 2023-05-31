using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using System.Threading;

namespace HB.FullStack.Cache.CacheItems
{
    public interface ICachedItemCache
    {
        Task<byte[]?> GetAsync(string key, CancellationToken token = default);

        /// <summary>
        /// 如果timestamp小于Cache中的，则Set失败
        /// </summary>
        Task<bool> SetAsync(string key, byte[] value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// 返回是否找到了
        /// </summary>
        Task<bool> RemoveAsync(string key, CancellationToken token = default);

        Task<bool> RemoveAsync(string[] keys, CancellationToken token = default);
    }
}
