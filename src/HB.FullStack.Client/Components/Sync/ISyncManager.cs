using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.Base;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.Sync
{
    /// <summary>
    /// 功能：1， 记录离线数据；2，网络恢复后，自动同步离线数据；3，接受服务器推来的数据
    /// </summary>
    public interface ISyncManager
    {
        event Func<Task>? Syncing;

        event Func<Task>? Synced;

        void Initialize();

        SyncStatus Status { get; }

        #region 手动控制

        Task ReSyncAsync();

        void WaitUntilNotSyncing();

        void EnsureSynced();

        #endregion

        #region Offline Data

        Task RecordOfflineAddAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : IDbModel;

        Task RecordOfflineUpdateAsync<TModel>(IEnumerable<PropertyChangePack> cps, TransactionContext transactionContext) where TModel : IDbModel;

        Task RecordOfflineDeleteAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : IDbModel;

        #endregion
    }
}