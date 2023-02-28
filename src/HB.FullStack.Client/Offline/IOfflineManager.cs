﻿using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;

namespace HB.FullStack.Client.Offline
{
    public interface IOfflineManager
    {
        Task RecordOfflineAddAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        Task RecordOfflineUpdateAsync<TModel>(IEnumerable<PropertyChangePack> cps, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        Task RecordOfflineDeleteAsync<TModel>(IEnumerable<TModel> models, TransactionContext transactionContext) where TModel : ClientDbModel, new();

        Task ReSyncAsync();

        Task<bool> HasOfflineDataAsync();
    }
}