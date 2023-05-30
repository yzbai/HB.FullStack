using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Model操作不可以
    /// </summary>
    public partial interface IModelCache
    {
        Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TCacheModel : IModel;

        /// <summary>
        /// 并不把models作为一个整体看待，里面有的可能会因为timestamp冲突而不成功。
        /// 需要改变吗？
        /// </summary>
        Task<IEnumerable<bool>> SetModelsAsync<TCacheModel>(IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : IModel;

        Task RemoveModelsAsync<TCacheModel>(string keyName, IEnumerable keyValues, CancellationToken token = default); //where TCacheModel : ITimestampModel, new();
    }
}
