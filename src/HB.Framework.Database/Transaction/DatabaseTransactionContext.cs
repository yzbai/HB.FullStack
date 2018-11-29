using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database.Transaction
{

    public class DatabaseTransactionContext
    {
        public IDbTransaction Transaction { get; set; }

        public DatabaseTransactionStatus Status { get; set; }
    }
}
