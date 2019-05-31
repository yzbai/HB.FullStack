using HB.Component.Identity.Entity;
using HB.Framework.Database;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserClaimBiz
    {
        Task<IList<UserClaim>> GetAsync(string userGuid, TransactionContext transContext = null);
    }
}
