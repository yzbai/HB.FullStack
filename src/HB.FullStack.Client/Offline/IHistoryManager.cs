using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Database;

namespace HB.FullStack.Client.Offline
{
    public interface IHistoryManager
    {
        Task RecordOfflineHistryAsync<TModel>(IEnumerable<TModel> models, HistoryType historyType, TransactionContext transactionContext) where TModel : ClientDbModel, new();
    }
}