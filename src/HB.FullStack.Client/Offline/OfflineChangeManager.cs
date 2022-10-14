using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.Api;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Offline
{
    public class OfflineChangeManager : IOfflineChangeManager
    {
        private readonly IDatabase _database;
        private readonly IDbModelDefFactory _dbModelDefFactory;

        public OfflineChangeManager(IDatabase database, IDbModelDefFactory dbModelDefFactory)
        {
            _database = database;
            _dbModelDefFactory = dbModelDefFactory;
        }

        public async Task RecordOfflineChangesAsync<TModel>(
            IEnumerable<TModel> models,
            OfflineChangeType offlineChangeType,
            TransactionContext transactionContext) where TModel : ClientDbModel, new()
        {
            //TODO: 反复对同一个Guid进行修改，需要合并

            DbModelDef modelDef = _dbModelDefFactory.GetDef<TModel>()!;

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

                if (offlineChangeType == OfflineChangeType.Update)
                {
                    offlineChange.ChangedPackDto = model.GetChangedPack().ToDto();
                }
                else if (offlineChangeType == OfflineChangeType.Delete)
                {
                    offlineChange.DeletedObjectJson = SerializeUtil.ToJson(model);
                }

                offlineHistories.Add(offlineChange);
            }

            await _database.BatchAddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }

        public async Task ReSyncAsync()
        {
            var changes = await _database.RetrieveAllAsync<OfflineChange>(null, orderBy: nameof(OfflineChange.Id));

            throw new NotImplementedException();

            //处理Add


            //处理Update
            //处理Delete
        }
    }
}
