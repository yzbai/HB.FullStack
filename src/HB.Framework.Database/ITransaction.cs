using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Database
{
    public interface ITransaction
    {
        Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null);
        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : Entity;

        Task RollbackAsync(TransactionContext context);

        Task CommitAsync(TransactionContext context);
    }
}
