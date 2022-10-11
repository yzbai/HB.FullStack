using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
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
            OfflineChangeType historyType,
            TransactionContext transactionContext) where TModel : ClientDbModel, new()
        {
            //TODO: 反复对同一个Guid进行修改，需要合并

            DbModelDef modelDef = _dbModelDefFactory.GetDef<TModel>()!;

            List<OfflineChange> offlineHistories = new List<OfflineChange>();

            foreach (TModel model in models)
            {
                OfflineChangePack history = new OfflineChangePack
                {
                    ModelId = model.Id,
                    ModelFullName = modelDef.ModelFullName,
                    HistoryType = historyType,
                    Handled = false,
                };

                offlineHistories.Add(history);
            }

            await _database.BatchAddAsync(offlineHistories, "", transactionContext).ConfigureAwait(false);
        }
    }
}
