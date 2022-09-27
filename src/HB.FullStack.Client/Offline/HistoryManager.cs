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
    public class HistoryManager : IHistoryManager
    {
        private readonly IDatabase _database;
        private readonly IDbModelDefFactory _dbModelDefFactory;

        public HistoryManager(IDatabase database, IDbModelDefFactory dbModelDefFactory)
        {
            _database = database;
            _dbModelDefFactory = dbModelDefFactory;
        }

        public async Task RecordOfflineHistryAsync<TModel>(IEnumerable<TModel> models, HistoryType historyType, TransactionContext transactionContext) where TModel : ClientDbModel, new()
        {
            DbModelDef modelDef = _dbModelDefFactory.GetDef<TModel>()!;

            List<OfflineHistory> offlineHistories = new List<OfflineHistory>();

            foreach (TModel model in models)
            {
                OfflineHistory history = new OfflineHistory
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
