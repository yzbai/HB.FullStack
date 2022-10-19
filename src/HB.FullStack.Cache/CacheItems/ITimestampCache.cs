using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;


namespace HB.FullStack.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Model操作不可以 
    /// </summary>
    public interface ITimestampCache
    {
        Task<byte[]?> GetAsync(string key, CancellationToken token = default);

        /// <summary>
        /// timestamp即ICacheModel.Timestamp
        /// </summary>
        Task<bool> SetAsync(string key, byte[] value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// 返回是否找到了
        /// </summary>
        Task<bool> RemoveAsync(string key, CancellationToken token = default);

        Task<bool> RemoveAsync(string[] keys, CancellationToken token = default);
    }
}
