using HB.Framework.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entities;
using System;

namespace HB.Component.Identity
{
    internal class UserClaimBiz : IUserClaimBiz
    {
        private readonly IDatabase _db;

        public UserClaimBiz(IDatabase database)
        {
            _db = database;
        }

        public Task<IEnumerable<TUserClaim>> GetAsync<TUserClaim>(string userGuid, TransactionContext? transContext = null) where TUserClaim : IdentityUserClaim, new()
        {
            return _db.RetrieveAsync<TUserClaim>(uc => uc.UserGuid == userGuid, transContext);
        }
    }
}
