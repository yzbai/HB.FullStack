using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database
{
    public enum DbTransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }

    public class DbTransactionContext
    {
        public IDbTransaction Transaction { get; set; }

        public DbTransactionStatus Status { get; set; }
    }
}
