using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    public interface IUserClaimBiz
    {
        Task<IList<UserClaim>> GetUserClaimsByUserIdAsync(long userId, DbTransactionContext transContext = null);
    }
}
