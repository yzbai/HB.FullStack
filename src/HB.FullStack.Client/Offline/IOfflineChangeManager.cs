using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Database;

namespace HB.FullStack.Client.Offline
{
    public interface IOfflineChangeManager
    {
        Task RecordOfflineChangesAsync<TModel>(IEnumerable<TModel> models, OfflineChangeType offlineChangeType, TransactionContext transactionContext) where TModel : ClientDbModel, new();
    }
}