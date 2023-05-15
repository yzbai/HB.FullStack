using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    //TODO: 实现 多数据库事务： TransactionScope
    public interface ITransaction
    {
        Task<TransactionContext> BeginTransactionAsync(DbSchema dbSchema, IsolationLevel? isolationLevel = null);

        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : BaseDbModel;

        Task RollbackAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0);

        Task CommitAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0);
    }
}