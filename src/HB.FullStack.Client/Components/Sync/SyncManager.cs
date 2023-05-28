using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.Base;
using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.Sync
{
    internal class SyncManager : ISyncManager
    {
        private readonly WeakAsyncEventManager _eventManager = new WeakAsyncEventManager();
        private readonly IDatabase _database;
        private readonly IModelDefFactory _modelDefFactory;
        private readonly IClientEvents _clientEvents;

        public event Func<Task>? Syncing { add => _eventManager.Add(value, nameof(Syncing)); remove => _eventManager.Remove(value, nameof(Syncing)); }

        public event Func<Task>? Synced { add => _eventManager.Add(value, nameof(Synced)); remove => _eventManager.Remove(value, nameof(Synced)); }

        public SyncManager(IDatabase database, IModelDefFactory modelDefFactory, IClientEvents clientEvents)
        {
            _database = database;
            _modelDefFactory = modelDefFactory;
            _clientEvents = clientEvents;
        }

        public void Initialize()
        {
            _clientEvents.ServerConnectResumed += () => ReSyncAsync();
        }

        //判断是否正在进行同步操作
        private readonly EventWaitHandle _notSyncingWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset, nameof(SyncManager));

        public void WaitUntilNotSyncing()
        {
            _notSyncingWaitHandle.WaitOne();
        }

        public async Task ReSyncAsync()
        {
            if (!_clientEvents.ServerConnected)
            {
                return;
            }

            WaitUntilNotSyncing();

            Status = SyncStatus.Syncing;

            _notSyncingWaitHandle.Reset();

            try
            {
                //TODO: 反复对同一个Guid进行修改，需要合并

                IEnumerable<OfflineChange> changes = await _database.RetrieveAsync<OfflineChange>(
                    t => t.Status == OfflineChangeStatus.Waiting || t.Status == OfflineChangeStatus.Failed, null, orderBy: nameof(OfflineChange.Id));

                if (changes.IsNullOrEmpty())
                {
                    return;
                }

                //TODO: 处理

                //处理Add

                //处理Update
                //处理Delete

                //
                Status = SyncStatus.Synced;
            }
            catch
            {
                Status = SyncStatus.SynceFailed;
            }
            finally
            {
                _notSyncingWaitHandle.Set();
            }

            //TODO: 处理失败的情况
        }

        public SyncStatus Status { get; private set; }

        public void EnsureSynced()
        {
            WaitUntilNotSyncing();

            if (Status != SyncStatus.Synced)
            {
                ReSyncAsync().SafeFireAndForget(ex =>
                {
                    //TODO: ex
                });
            }
        }

        #region Offline Data

        public Task RecordOfflineAddAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : IDbModel
            => RecordOfflineAddOrDeleteAsync(models, OfflineChangeType.Add, transactionContext);

        public Task RecordOfflineDeleteAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : IDbModel
            => RecordOfflineAddOrDeleteAsync(models, OfflineChangeType.Delete, transactionContext);

        private async Task RecordOfflineAddOrDeleteAsync<TModel>(IEnumerable<TModel> models, OfflineChangeType offlineChangeType, TransactionContext transactionContext) where TModel : IDbModel
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<TModel>(ModelKind.Db) as DbModelDef;

            ThrowIf.Null(modelDef, nameof(modelDef));

            List<OfflineChange> offlineHistories = new List<OfflineChange>();

            foreach (TModel model in models)
            {
                OfflineChange offlineChange = new OfflineChange
                {
                    Type = offlineChangeType,
                    Status = OfflineChangeStatus.Waiting,
                    ModelIdJson = SerializeUtil.ToJson(model.Id.ThrowIfNull(nameof(model.Id))),
                    ModelFullName = modelDef.FullName
                };

                offlineHistories.Add(offlineChange);
            }

            await _database.AddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        public async Task RecordOfflineUpdateAsync<TModel>(IEnumerable<PropertyChangePack> cps, TransactionContext transactionContext) where TModel : IDbModel
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<TModel>(ModelKind.Db) as DbModelDef;

            ThrowIf.Null(modelDef, nameof(modelDef));

            List<OfflineChange> offlineHistories = new List<OfflineChange>();

            foreach (var changedPack in cps)
            {
                OfflineChange offlineChange = new OfflineChange
                {
                    Type = OfflineChangeType.UpdateProperties,
                    Status = OfflineChangeStatus.Waiting,
                    ModelIdJson = changedPack.AddtionalProperties["Id"].To(modelDef.PrimaryKeyPropertyDef.Type),
                    ModelFullName = modelDef.FullName,
                    ChangePack = changedPack
                };

                offlineHistories.Add(offlineChange);
            }

            await _database.AddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        private async Task<bool> HasOfflineDataAsync()
        {
            WaitUntilNotSyncing();

            long count = await _database.CountAsync<OfflineChange>(t => t.Status == OfflineChangeStatus.Waiting || t.Status == OfflineChangeStatus.Failed, null).ConfigureAwait(false);

            return count != 0;
        }

        #endregion
    }
}