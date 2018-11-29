using HB.Framework.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database.Transaction;

namespace HB.Component.Identity
{
    public class UserClaimBiz : IUserClaimBiz
    {
        private IDatabase _db;
        private readonly ILogger _logger;

        public UserClaimBiz(IDatabase database, ILogger<UserClaimBiz> logger)
        {
            _db = database;
            _logger = logger;
        }

        public Task<IList<UserClaim>> GetUserClaimsByUserIdAsync(long userId, DatabaseTransactionContext transContext = null)
        {
            return _db.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
        }
    }
}
