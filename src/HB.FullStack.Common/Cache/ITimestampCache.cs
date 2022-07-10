

using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


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
