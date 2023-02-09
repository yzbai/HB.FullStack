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

        /// <summary>
        /// Update Fields for TimestampDbModel. Using timestamp method optimistic locking.
        /// </summary>
        /// <param name="timestamp">TimestampDbModel.Timestamp</param>
        /// <param name="newTimestamp">为null时，由系统指定</param>
        Task UpdatePropertiesAsync<T>(
            object id,
            IList<(string propertyName, object? propertyValue)> propertyNameValues,
            long timestamp,
            string lastUser,
            TransactionContext? transContext,
            long? newTimestamp = null) where T : TimestampDbModel, new();

        Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<(string propertyName, object? propertyValue)> properties, long oldTimestamp, long? newTimestamp)> modelChanges,
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
            TransactionContext? transContext,
            long? newTimestamp = null) where T : DbModel, new();

        Task BatchUpdatePropertiesAsync<T>(
            IList<(object id, IList<(string propertyNames, object? oldPropertyValues, object? newPropertyValues)> properties, long? newTimestamp)> modelChanges,
            string lastUser,
            TransactionContext? transactionContext = null) where T : DbModel, new();
        /// <summary>
        /// 如果ChangedPack中包含Timestamp，也不会判断oldTimestamp是否等于数据库中的Timestamp，但会更新数据库Timestamp为newTimestamp
        /// </summary>
        Task UpdatePropertiesAsync<T>(
            ChangedPack changedPropertyPack,
            string lastUser,
            TransactionContext? transContext) where T : DbModel, new();

        /// <summary>
        /// 如果ChangedPack中包含Timestamp，也不会判断oldTimestamp是否等于数据库中的Timestamp，但会更新数据库Timestamp为newTimestamp
        /// </summary>
        Task BatchUpdatePropertiesAsync<T>(
            IEnumerable<ChangedPack> changedPacks,
            string lastUser,
            TransactionContext? transContext) where T : DbModel, new();

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