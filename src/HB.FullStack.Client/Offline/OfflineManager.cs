using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Offline
{
    public class OfflineManager : IOfflineManager
    {
        private readonly IDatabase _database;
        private readonly IModelDefFactory _modelDefFactory;
        private readonly IStatusManager _statusManager;

        public OfflineManager(IDatabase database, IModelDefFactory modelDefFactory, IStatusManager statusManager)
        {
            _database = database;
            _modelDefFactory = modelDefFactory;
            _statusManager = statusManager;

            _statusManager.Syncing += ReSyncAsync;
        }

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

            await _database.BatchAddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
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
                    ModelId = SerializeUtil.To<Guid>( changedPack.AddtionalProperties["Id"])!,
                    ModelFullName = modelDef.ModelFullName,
                    ChangePack = changedPack
                };


                offlineHistories.Add(offlineChange);
            }

            await _database.BatchAddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        public async Task<bool> HasOfflineDataAsync()
        {
            _statusManager.WaitUntilSynced();

            long count = await _database.CountAsync<OfflineChange>(null).ConfigureAwait(false);

            return count != 0;
        }

        public async Task ReSyncAsync()
        {

            //TODO: 反复对同一个Guid进行修改，需要合并

            var changes = await _database.RetrieveAllAsync<OfflineChange>(null, orderBy: nameof(OfflineChange.Id));

            throw new NotImplementedException();

            //处理Add


            //处理Update
            //处理Delete
        }
    }
}
