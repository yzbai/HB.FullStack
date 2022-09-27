﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public interface IDatabaseWriter
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        /// <summary>
        /// Update Entire Model. 
        /// Using timestamp method optimistic locking if a TimestampDbModel.
        /// Otherwise, maybe have data conflict.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        /// <summary>
        /// Update Fields for TimestampDbModel. Using timestamp method optimistic locking.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="propertyNameValues"></param>
        /// <param name="timestamp">TimestampDbModel.Timestamp</param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        Task UpdateFieldsAsync<T>(
            object id,
            IList<(string propertyName, object? propertyValue)> propertyNameValues,
            long timestamp,
            string lastUser,
            TransactionContext? transContext) where T : TimestampDbModel, new();

        /// <summary>
        /// Update Fields for DbModel. Using property value compare method optimistic locking
        /// </summary>
        /// <param name="propertyOldNewValues">(propertyName-oldvalue-newvalue）</param>
        Task UpdateFieldsAsync<T>(
            object id,
            IList<(string propertyName, object? oldValue, object? newValue)> propertyNameOldNewValues,
            string lastUser,
            TransactionContext? transContext) where T : DbModel, new();

        /// <summary>
        /// Delete DbModel. Using timestamp method optimistic locking if a TimestampDbModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        /// <summary>
        /// Delete TimelessDbModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : TimelessDbModel, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DbModel, new();

        Task SetByIdAsync<T>(T item, /*string lastUser,*/ TransactionContext? transContext = null) where T : TimelessDbModel, new();

        Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, TransactionContext? transContext) where T : TimelessDbModel, new();
    }
}