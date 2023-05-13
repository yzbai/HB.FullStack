﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public interface IDbWriter
    {
        #region Add

        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new();

        Task AddAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel, new();

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// If item is IPropertyTrackableObject, will use <see cref="UpdatePropertiesAsync{T}(PropertyChangeJsonPack, string, TransactionContext?)"/> Underneath.
        /// If item is TimestampDBModel, will use Timestamp to solve conflict.
        /// If item is TimelessDBModel, will just update, ignore conflict.
        /// </summary>
        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new();

        /// <summary>
        /// Update.
        /// If item is IPropertyTrackableObject, will use <see cref="UpdatePropertiesAsync{T}(IEnumerable{PropertyChangeJsonPack}, string, TransactionContext?)"/> Underneath.
        /// If item is TimestampDBModel, will use Timestamp to solve conflict.
        /// If item is TimelessDBModel, will just update, ignore conflict.
        /// </summary>
        Task UpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        #endregion

        #region Update Properties

        Task UpdatePropertiesAsync<T>(TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, ITimestamp, new();

        Task UpdatePropertiesAsync<T>(IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel, ITimestamp, new();

        Task UpdatePropertiesAsync<T>(OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new();

        Task UpdatePropertiesAsync<T>(IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel, new();

        Task UpdatePropertiesAsync<T>(IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T: BaseDbModel, new();

        Task UpdatePropertiesAsync<T>(IList<IgnoreConflictCheckUpdatePack> updatePack, string lastUser, TransactionContext transContext) where T:BaseDbModel, new();

        Task UpdatePropertiesAsync<T>(PropertyChangePack changePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel, new();

        Task UpdatePropertiesAsync<T>(IList<PropertyChangePack> changePacks, string lastUser, TransactionContext transContext) where T : BaseDbModel, new();

        #endregion

        #region Delete

        /// <summary>
        /// UpdateDeletedFields DbModel. Using timestamp method optimistic locking if a TimestampDbModel
        /// </summary>
        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new();

        Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new();

        Task DeleteAsync<T>(object id, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new();

        /// <summary>
        /// UpdateDeletedFields TimelessDbModel without conflict check
        /// </summary>
        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, string lastUser, TransactionContext? transactionContext = null, bool? trulyDelete = null) where T : TimelessDbModel, new();

        Task DeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : DbModel, new();

        Task DeleteAsync<T>(IList<object> ids, TransactionContext? transContext, string lastUser, bool? trulyDelete = null) where T : TimelessDbModel, new();

        Task DeleteAsync<T>(IList<object> ids, IList<long?> timestamps, string lastUser, TransactionContext? transContext, bool? trulyDelete = null) where T : TimestampDbModel, new();

        #endregion

        #region AddOrUpdate

        Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : TimelessDbModel, new();

        Task AddOrUpdateByIdAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : TimelessDbModel, new();

        #endregion
    }
}