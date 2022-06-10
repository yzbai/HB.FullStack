﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;

namespace HB.FullStack.Repository
{
    public abstract class KVStoreEntityRepository<TEntity> where TEntity : KVStoreEntity, new()
    {
        protected IKVStore KVStore { get; }

        protected KVStoreEntityRepository(IKVStore kVStore)
        {
            KVStore = kVStore;
        }

        public Task<TEntity?> GetAsync(object key)
        {
            return KVStore.GetAsync<TEntity>(key.ToString()!);
        }

        /// <summary>
        /// AddAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        
        public Task AddAsync(TEntity entity, string lastUser)
        {
            return KVStore.AddAsync(entity, lastUser);
        }

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        
        public Task UpdateAsync(TEntity entity, string lastUser)
        {
            return KVStore.UpdateAsync(entity, lastUser);
        }

        public Task DeleteAsync(TEntity entity, string lastUser)
        {
            entity.LastUser = lastUser;
            string key = KVStore.GetEntityKey(entity);
            return KVStore.DeleteAsync<TEntity>(key, entity.Version);
        }
    }
}
