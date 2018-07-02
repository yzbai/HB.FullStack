using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;
using HB.Framework.Database;

namespace HB.Component.Identity.Abstractions
{
    public interface IClaimsPrincipalFactory
    {
        Task<IList<Claim>> CreateClaimsAsync(User user, DbTransactionContext transContext = null);
    }
}