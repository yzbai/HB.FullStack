﻿using System;
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
    public abstract class KVStoreModelRepository<TModel>/* : IModelRepo<TModel>*/ where TModel : KVStoreModel, new()
    {
        protected IKVStore KVStore { get; }

        protected KVStoreModelRepository(IKVStore kVStore)
        {
            KVStore = kVStore;
        }

     

        public Task<TModel?> GetAsync(object key)
        {
            return KVStore.GetAsync<TModel>(key);
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
            return KVStore.DeleteAsync<TModel>(model);
        }

     
    }
}
