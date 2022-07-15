using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;


namespace HB.FullStack.Common.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Model操作不可以 
    /// </summary>
    public interface ICollectionCache
    {
        Task<byte[]?> GetFromCollectionAsync(string collectionKey, string itemKey, CancellationToken token = default);

        Task<bool> SetToCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<byte[]> itemValues, IEnumerable<long> timestamps, DistributedCacheEntryOptions options, CancellationToken token = default);

        Task RemoveFromCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, CancellationToken token = default);

        Task<bool> RemoveCollectionAsync(string collectionKey, CancellationToken token = default);
    }
}
