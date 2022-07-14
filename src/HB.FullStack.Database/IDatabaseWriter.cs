﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Database.DatabaseModels;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{
    public interface IDatabaseWriter
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task UpdateFieldsAsync<T>(object id, IList<(string, object?)> propertyNameValues, long curTimestamp, string lastUser, TransactionContext? transContext) where T : TimestampDBModel, new();

        /// <summary>
        /// 通过比较oldvalue来实现字段力度乐观锁.
        /// </summary>
        /// <param name="propertyOldNewValues">(propertyName-oldvalue-newvalue）</param>
        Task UpdateFieldsAsync<T>(object id, IList<(string, object?, object?)> propertyNameOldNewValues, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task DeleteAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transactionContext = null) where T : TimeLessDBModel, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DBModel, new();

        Task SetByIdAsync<T>(T item, /*string lastUser,*/ TransactionContext? transContext = null) where T : TimeLessDBModel, new();

        Task BatchAddOrUpdateByIdAsync<T>(IEnumerable<T> items, TransactionContext? transContext) where T : TimeLessDBModel, new();
    }
}