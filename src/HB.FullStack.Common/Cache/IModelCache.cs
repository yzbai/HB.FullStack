

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
    public partial interface IModelCache
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCacheModel"></typeparam>
        /// <param name="keyName">primarykey or dimensionKey</param>
        /// <param name="keyValues"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TCacheModel : Common.Cache.CacheModels.ICacheModel, new();

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveModelsAsync<TCacheModel>(string dimensionKeyName, IEnumerable dimensionKeyValues, CancellationToken token = default) where TCacheModel : Common.Cache.CacheModels.ICacheModel, new();

        /// <summary>
        /// 返回是否成功更新。false是数据版本小于缓存中的
        /// </summary>
        Task<IEnumerable<bool>> SetModelsAsync<TCacheModel>(IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : Common.Cache.CacheModels.ICacheModel, new();
    }
}
