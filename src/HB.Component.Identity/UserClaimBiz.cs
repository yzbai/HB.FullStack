using HB.Framework.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;

namespace HB.Component.Identity
{
    public class UserClaimBiz : BizWithDbTransaction, IUserClaimBiz
    {
        private IDatabase _db;
        private ILogger _logger;

        public UserClaimBiz(IDatabase database, ILogger<UserClaimBiz> logger)
            : base(database)
        {
            _db = database;
            _logger = logger;
        }

        public Task<IList<UserClaim>> GetUserClaimsByUserIdAsync(long userId, DbTransactionContext transContext = null)
        {
            return _db.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
        }
    }
}
