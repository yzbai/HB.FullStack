using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.KVStoreModels;
using HB.FullStack.Server.Repository;

namespace HB.FullStack.Repository
{
    internal abstract class KVStoreModelRepository<TModel> : IModelRepo<TModel> where TModel : KVStoreModel, new()
    {
        protected IKVStore KVStore { get; }

        protected KVStoreModelRepository(IKVStore kVStore)
        {
            KVStore = kVStore;
        }

        public event Func<object, ModelChangeEventArgs, Task>? ModelUpdating;
        public event Func<object, ModelChangeEventArgs, Task>? ModelUpdated;
        public event Func<object, ModelChangeEventArgs, Task>? ModelUpdateFailed;
        public event Func<object, ModelChangeEventArgs, Task>? ModelAdding;
        public event Func<object, ModelChangeEventArgs, Task>? ModelAdded;
        public event Func<object, ModelChangeEventArgs, Task>? ModelAddFailed;
        public event Func<object, ModelChangeEventArgs, Task>? ModelDeleting;
        public event Func<object, ModelChangeEventArgs, Task>? ModelDeleted;
        public event Func<object, ModelChangeEventArgs, Task>? ModelDeleteFailed;

        public Task<TModel?> GetAsync(object key)
        {
            return KVStore.GetAsync<TModel>(key.ToString()!);
        }
        
        public Task AddAsync(TModel model, string lastUser)
        {
            return KVStore.AddAsync(model, lastUser);
        }

        public Task UpdateAsync(TModel model, string lastUser)
        {
            return KVStore.UpdateAsync(model, lastUser);
        }

        public Task DeleteAsync(TModel model, string lastUser)
        {
            model.LastUser = lastUser;
            string key = KVStore.GetModelKey(model);
            return KVStore.DeleteAsync<TModel>(key, model.Timestamp);
        }

        public void RegisterModelChangedEvents(Func<object, ModelChangeEventArgs, Task> OnModelsChanged)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task AddAsync<T>(IList<T> models, string lastUser, TransactionContext transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(IList<T> models, string lastUser, TransactionContext transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync<T>(IList<T> models, string lastUser, TransactionContext transContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task UpdateProperties<T>(PropertyChangePack cp, string lastUser, TransactionContext? transactionContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public Task UpdateProperties<T>(IList<PropertyChangePack> cps, string lastUser, TransactionContext transactionContext) where T : IDbModel
        {
            throw new NotImplementedException();
        }

        public void InvalidateCache(ICachedItem cachedItem)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCache(IEnumerable<ICachedItem> cachedItems)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCache(ICachedCollectionItem cachedCollectionItem)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCache(IEnumerable<ICachedCollectionItem> cachedCollectionItems)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCacheCollection<T>() where T : ICachedCollectionItem
        {
            throw new NotImplementedException();
        }
    }
}
