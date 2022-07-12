using HB.FullStack.Common;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace HB.FullStack.Common.Cache.CacheModels
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Model操作不可以
    /// </summary>
    public partial interface IModelCache2
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCacheModel"></typeparam>
        /// <param name="keyName">primarykey or dimensionKey</param>
        /// <param name="keyValues"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TCacheModel : ICacheModel, new();

        Task RemoveModelsAsync<TCacheModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TCacheModel : ICacheModel, new();

        /// <summary>
        /// 并不把models作为一个整体看待，里面有的可能会因为timestamp冲突而不成功。
        /// 需要改变吗？
        /// </summary>
        Task<IEnumerable<bool>> SetModelsAsync<TCacheModel>(IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : ICacheModel, new();
    }
}
