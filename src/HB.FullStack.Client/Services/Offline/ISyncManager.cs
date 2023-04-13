using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;

namespace HB.FullStack.Client.Services.Offline
{
    public enum SyncStatus
    {
        NotSynced_OfflineData,
        NotSynced_ServerPush,
        Syncing,
        SynceFailed,//失败 
        Synced //成功
    }

    /// <summary>
    /// 功能：1， 记录离线数据；2，网络恢复后，自动同步离线数据；3，接受服务器推来的数据
    /// </summary>
    public interface ISyncManager
    {
        event Func<Task>? Syncing;

        event Func<Task>? Synced;

        void InitializeAsync();

        SyncStatus Status { get; }

        #region 手动控制

        Task ReSyncAsync();

        //void WaitUntilSynced();

        void EnsureSynced();

        #endregion

        #region Offline Data

        Task RecordOfflineAddAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        Task RecordOfflineUpdateAsync<TModel>(IEnumerable<PropertyChangePack> cps, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        Task RecordOfflineDeleteAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        #endregion
    }
}