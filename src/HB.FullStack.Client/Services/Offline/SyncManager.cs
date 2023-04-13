using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.Services;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Services.Offline
{
    public class SyncManager : ISyncManager
    {
        private readonly WeakEventManager _eventManager = new WeakEventManager();
        private readonly IDatabase _database;
        private readonly IModelDefFactory _modelDefFactory;
        private readonly INetwork _network;

        public event Func<Task>? Syncing
        {
            add => _eventManager.AddEventHandler(value, nameof(Syncing));
            remove => _eventManager.RemoveEventHandler(value, nameof(Syncing));
        }
        public event Func<Task>? Synced
        {
            add=> _eventManager.AddEventHandler(value, nameof(Synced));
            remove=> _eventManager.RemoveEventHandler(value, nameof(Synced));
        }

        public SyncManager(IDatabase database, IModelDefFactory modelDefFactory, INetwork network)
        {
            _database = database;
            _modelDefFactory = modelDefFactory;
            _network = network;

            _network.NetworkResumed += () => ReSyncAsync();
        }

        public async Task InitializeAsync()
        {
            await ReSyncAsync();
        }

        #region Offline Data

        public Task RecordOfflineAddAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new()
            => RecordOfflineAddOrDeleteAsync(models, OfflineChangeType.Add, transactionContext);

        public Task RecordOfflineDeleteAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new()
            => RecordOfflineAddOrDeleteAsync(models, OfflineChangeType.Delete, transactionContext);

        private async Task RecordOfflineAddOrDeleteAsync<TModel>(IEnumerable<TModel> models, OfflineChangeType offlineChangeType, TransactionContext transactionContext) where TModel : ClientDbModel, new()
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<TModel>(ModelKind.Db) as DbModelDef;

            ThrowIf.Null(modelDef, nameof(modelDef));

            List<OfflineChange> offlineHistories = new List<OfflineChange>();

            foreach (TModel model in models)
            {
                OfflineChange offlineChange = new OfflineChange
                {
                    Type = offlineChangeType,
                    Status = OfflineChangeStatus.Pending,
                    ModelId = model.Id,
                    ModelFullName = modelDef.ModelFullName
                };

                offlineHistories.Add(offlineChange);
            }

            await _database.AddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        public async Task RecordOfflineUpdateAsync<TModel>(IEnumerable<PropertyChangePack> cps, TransactionContext transactionContext) where TModel : ClientDbModel, new()
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<TModel>(ModelKind.Db) as DbModelDef;

            ThrowIf.Null(modelDef, nameof(modelDef));

            List<OfflineChange> offlineHistories = new List<OfflineChange>();

            foreach (var changedPack in cps)
            {
                OfflineChange offlineChange = new OfflineChange
                {
                    Type = OfflineChangeType.UpdateProperties,
                    Status = OfflineChangeStatus.Pending,
                    ModelId = changedPack.AddtionalProperties["Id"].To<Guid>()!,
                    ModelFullName = modelDef.ModelFullName,
                    ChangePack = changedPack
                };


                offlineHistories.Add(offlineChange);
            }

            await _database.AddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        private async Task<bool> HasOfflineDataAsync()
        {
            _notSyncingWaitHandle.WaitOne();

            _statusManager.WaitUntilSynced();

            long count = await _database.CountAsync<OfflineChange>(null).ConfigureAwait(false);

            return count != 0;
        }

        #endregion

        //判断是否正在进行同步操作
        private readonly EventWaitHandle _notSyncingWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, nameof(SyncManager));

        public async Task ReSyncAsync()
        {

            //TODO: 反复对同一个Guid进行修改，需要合并

            var changes = await _database.RetrieveAllAsync<OfflineChange>(null, orderBy: nameof(OfflineChange.Id));

            throw new NotImplementedException();

            //处理Add


            //处理Update
            //处理Delete
        }



        public SyncStatus SyncStatus => throw new NotImplementedException();

        public void WaitUntilSynced()
        {
            throw new NotImplementedException();
        }

        public void EnsureSynced()
        {
            _notSyncingWaitHandle.WaitOne();

            if (SyncStatus != SyncStatus.Synced)
            {

            }
        }
    }
}
