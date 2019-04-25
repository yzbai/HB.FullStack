using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using System;
using HB.Framework.Database.Transaction;

namespace HB.Component.Identity
{
    public class RoleBiz : IRoleBiz
    {
        private readonly IDatabase _database;
        private readonly ILogger _logger;

        public RoleBiz(IDatabase database, ILogger<RoleBiz> logger)
        {
            _database = database;
            _logger = logger;
        }

        public Task<IEnumerable<string>> GetUserRoleNamesAsync(long userId, DatabaseTransactionContext transContext = null)
        {
            FromExpression<Role> from = _database.NewFrom<Role>().RightJoin<UserRole>((r, ru) => r.Id == ru.RoleId);
            WhereExpression<Role> where = _database.NewWhere<Role>().And<UserRole>(ru => ru.UserId == userId);

            return _database.RetrieveAsync<Role>(from, where, transContext)
                .ContinueWith(o=>o.Result.Select(r=>r.Name), TaskScheduler.Default);
        }

        public Task<int> GetRoleByNameAsync(string roleName, DatabaseTransactionContext transContext = null)
        {
            //动用Cache
            return Task.FromResult(0);
        }
    }
}
