using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Repository;

namespace HB.FullStack.Server.Repository
{
    public interface IModelRepo<T> where T : IModel
    {
        #region Events

        event Func<object, ModelChangeEventArgs, Task>? ModelUpdating;
        event Func<object, ModelChangeEventArgs, Task>? ModelUpdated;
        event Func<object, ModelChangeEventArgs, Task>? ModelUpdateFailed;
        event Func<object, ModelChangeEventArgs, Task>? ModelAdding;
        event Func<object, ModelChangeEventArgs, Task>? ModelAdded;
        event Func<object, ModelChangeEventArgs, Task>? ModelAddFailed;
        event Func<object, ModelChangeEventArgs, Task>? ModelDeleting;
        event Func<object, ModelChangeEventArgs, Task>? ModelDeleted;
        event Func<object, ModelChangeEventArgs, Task>? ModelDeleteFailed;

        void RegisterModelChangedEvents(Func<object, ModelChangeEventArgs, Task> OnModelsChanged);

        #endregion

        #region CRUD

        Task AddAsync(T model, string lastUser, TransactionContext? transContext) ;
        Task AddAsync(IList<T> models, string lastUser, TransactionContext transContext) ;
        Task DeleteAsync(T model, string lastUser, TransactionContext? transContext) ;
        Task DeleteAsync(IList<T> models, string lastUser, TransactionContext transContext) ;

        Task UpdateAsync(T model, string lastUser, TransactionContext? transContext) ;
        Task UpdateAsync(IList<T> models, string lastUser, TransactionContext transContext) ;
        Task UpdateProperties(PropertyChangePack cp, string lastUser, TransactionContext? transactionContext) ;
        Task UpdateProperties(IList<PropertyChangePack> cps, string lastUser, TransactionContext transactionContext) ;

        #endregion

        #region Cache

        void InvalidateCache(ICachedItem cachedItem);
        void InvalidateCache(IEnumerable<ICachedItem> cachedItems);
        void InvalidateCache(ICachedCollectionItem cachedCollectionItem);
        void InvalidateCache(IEnumerable<ICachedCollectionItem> cachedCollectionItems);
        void InvalidateCacheCollection<TCachedCollectionItem>() where TCachedCollectionItem : ICachedCollectionItem;

        #endregion

    }
}
