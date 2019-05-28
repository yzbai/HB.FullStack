using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database.Transaction
{
    public enum DatabaseTransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }

    public class DatabaseTransactionContext
    {
        public IDbTransaction Transaction { get; set; }

        public DatabaseTransactionStatus Status { get; set; }
    }
}
