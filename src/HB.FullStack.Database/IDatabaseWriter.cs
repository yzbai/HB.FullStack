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
    public interface IDatabaseWriter
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

        /// <summary>
        /// Update Fields for TimestampDbModel. Using timestamp method optimistic locking.
        /// </summary>
        /// <param name="timestamp">TimestampDbModel.Timestamp</param>
        Task UpdatePropertiesAsync<T>(
            object id,
            IList<(string propertyName, object? propertyValue)> propertyNameValues,
            long timestamp,
            string lastUser,
            TransactionContext? transContext) where T : TimestampDbModel, new();

        Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<string> propertyNames, IList<object?> propertyValues, long timestamp)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext) where T : TimestampDbModel, new();

        /// <summary>
        /// Update Fields for DbModel. Using property value compare method optimistic locking
        /// </summary>
        /// <param name="propertyOldNewValues">(propertyName-oldvalue-newvalue）</param>
        Task UpdatePropertiesAsync<T>(
            object id,
            IList<(string propertyName, object? oldValue, object? newValue)> propertyNameOldNewValues,
            string lastUser,
            TransactionContext? transContext) where T : DbModel, new();

        Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext = null) where T : DbModel, new();

        Task UpdatePropertiesAsync<T>(ChangedPack changedPropertyPack, string lastUser, TransactionContext? transContext)
            where T : DbModel, new();

        Task BatchUpdatePropertiesAsync<T>(IList<ChangedPack> changedPacks, string lastUser, TransactionContext? transContext)
            where T : DbModel, new();

        #endregion

        #region Delete

        /// <summary>
        /// UpdateDeletedFields DbModel. Using timestamp method optimistic locking if a TimestampDbModel
        /// </summary>
        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext, bool trulyDelete = false)
            where T : DbModel, new();

        Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext, bool trulyDelete = false)
            where T : TimestampDbModel, new();

        Task DeleteAsync<T>(object id, string lastUser, TransactionContext? transContext, bool trulyDelete = false)
            where T : TimelessDbModel, new();

        /// <summary>
        /// UpdateDeletedFields TimelessDbModel without conflict check
        /// </summary>
        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, string lastUser, TransactionContext? transactionContext = null, bool trulyDelete = false)
            where T : TimelessDbModel, new();

        Task BatchDeleteAsync<T>(IList<T> items, string lastUser, TransactionContext? transContext, bool trulyDelete = false) where T : DbModel, new();

        Task BatchDeleteAsync<T>(IList<object> ids, string lastUser, TransactionContext? transContext, bool trulyDelete = false) where T : TimelessDbModel, new();

        Task BatchDeleteAsync<T>(IList<object> ids,
            IList<long?> timestamps, string lastUser, TransactionContext? transContext, bool trulyDelete = false) where T : TimestampDbModel, new();

        #endregion  

        Task AddOrUpdateByIdAsync<T>(T item, /*string lastUser,*/ TransactionContext? transContext = null) where T : TimelessDbModel, new();

        Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, TransactionContext? transContext) where T : TimelessDbModel, new();
    }
}