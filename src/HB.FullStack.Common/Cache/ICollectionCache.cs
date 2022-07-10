﻿

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
    public interface ICollectionCache
    { 
        Task<byte[]?> GetFromCollectionAsync(string collectionKey, string itemKey, CancellationToken token = default);

        Task<bool> SetToCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<byte[]> itemValues, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default);

        Task<bool> RemoveFromCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, long timestamp, CancellationToken token = default);

        Task<bool> RemoveCollectionAsync(string collectionKey, CancellationToken token = default);
    }
}
