using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database
{
    public enum TransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }

    public class TransactionContext
    {
        public IDbTransaction Transaction { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
