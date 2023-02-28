/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public interface IDbWriter
    {
        #region Add

        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        #endregion

        #region Update

        /// <summary>
        /// Update Entire Model. 
        /// Using timestamp method optimistic locking if a TimestampDbModel.
        /// Otherwise, maybe have data conflict.
        /// </summary>
        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        #endregion

        #region Update Properties

        Task UpdatePropertiesAsync<T>(UpdateUsingTimestamp updatePack, string lastUser, TransactionContext? transContext) where T : TimestampDbModel, new();

        Task BatchUpdatePropertiesAsync<T>(IList<UpdateUsingTimestamp> updatePacks, string lastUser, TransactionContext? transactionContext) where T : TimestampDbModel, new();

        Task UpdatePropertiesAsync<T>(UpdateUsingCompare updatePack, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task BatchUpdatePropertiesAsync<T>(IList<UpdateUsingCompare> updatePacks, string lastUser, TransactionContext? transactionContext) where T : DbModel, new();

        Task UpdatePropertiesAsync<T>(PropertyChangePack changePack, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task BatchUpdatePropertiesAsync<T>(IEnumerable<PropertyChangePack> changePacks, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        #endregion

        #region Delete

        /// <summary>
        /// UpdateDeletedFields DbModel. Using timestamp method optimistic locking if a TimestampDbModel
        /// </summary>
        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext, bool? trulyDelete = null)
            where T : DbModel, new();

        Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext, bool? trulyDelete = null)
            where T : TimestampDbModel, new();

        Task DeleteAsync<T>(object id, TransactionContext? transContext, string lastUser, bool? trulyDelete = null)
            where T : TimelessDbModel, new();

        /// <summary>
        /// UpdateDeletedFields TimelessDbModel without conflict check
        /// </summary>
        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, string lastUser, TransactionContext? transactionContext = null, bool? trulyDelete = null)
            where T : TimelessDbModel, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new();

        Task BatchDeleteAsync<T>(IList<object> ids, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new();

        Task BatchDeleteAsync<T>(IList<object> ids,
            IList<long?> timestamps, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new();

        #endregion

        #region AddOrUpdate

        Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : TimelessDbModel, new();

        Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : TimelessDbModel, new();

        #endregion
    }
}