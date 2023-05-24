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

        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task AddAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel;

        #endregion

        #region Update

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task UpdateAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel;

        #endregion

        #region Update Properties

        Task UpdatePropertiesAsync<T>(TimestampUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task UpdatePropertiesAsync<T>(IList<TimestampUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel;

        Task UpdatePropertiesAsync<T>(OldNewCompareUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task UpdatePropertiesAsync<T>(IList<OldNewCompareUpdatePack> updatePacks, string lastUser, TransactionContext transactionContext) where T : BaseDbModel;

        Task UpdatePropertiesAsync<T>(IgnoreConflictCheckUpdatePack updatePack, string lastUser, TransactionContext? transContext) where T: BaseDbModel;

        Task UpdatePropertiesAsync<T>(IList<IgnoreConflictCheckUpdatePack> updatePack, string lastUser, TransactionContext transContext) where T:BaseDbModel;

        Task UpdatePropertiesAsync<T>(PropertyChangePack changePack, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task UpdatePropertiesAsync<T>(IList<PropertyChangePack> changePacks, string lastUser, TransactionContext transContext) where T : BaseDbModel;

        #endregion

        #region Delete

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task DeleteAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel;

        Task DeleteAsync<T>(object id, long timestamp, string lastUser, TransactionContext? transContext) where T : BaseDbModel, ITimestamp;

        Task DeleteAsync<T>(IList<object> ids, IList<long> timestamps, string lastUser, TransactionContext transContext) where T : BaseDbModel, ITimestamp;
        
        Task DeleteIgnoreConflictCheckAsync<T>(object id, string lastUser, TransactionContext? transContext) where T : BaseDbModel;

        Task DeleteIgnoreConflictCheckAsync<T>(IList<object> ids, string lastUser, TransactionContext transContext) where T : BaseDbModel;
        
        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, string lastUser, TransactionContext transactionContext) where T : BaseDbModel;

        #endregion

        #region AddOrUpdate

        Task AddOrUpdateByIdAsync<T>(T item, string lastUser, TransactionContext? transContext = null) where T : BaseDbModel;

        Task AddOrUpdateByIdAsync<T>(IList<T> items, string lastUser, TransactionContext transContext) where T : BaseDbModel;

        #endregion
    }
}