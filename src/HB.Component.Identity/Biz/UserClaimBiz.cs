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
        private readonly IDatabase _db;
        private readonly ILogger _logger;

        public UserClaimBiz(IDatabase database, ILogger<UserClaimBiz> logger)
        {
            _db = database;
            _logger = logger;
        }

        public Task<IList<UserClaim>> GetAsync(string userGuid, TransactionContext transContext = null)
        {
            if (userGuid.IsNullOrEmpty())
            {
                return Task.FromResult((IList<UserClaim>)new List<UserClaim>());
            }
            return _db.RetrieveAsync<UserClaim>(uc => uc.UserGuid == userGuid, transContext);
        }
    }
}
