using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using System;

namespace HB.Component.Identity
{
    public class RoleBiz : BizWithDbTransaction, IRoleBiz
    {
        private IDatabase _database;
        private readonly ILogger _logger;

        public RoleBiz(IDatabase database, ILogger<RoleBiz> logger) : base(database)
        {
            _database = database;
            _logger = logger;
        }

        public Task<IEnumerable<string>> GetUserRoleNamesAsync(long userId, DbTransactionContext transContext = null)
        {
            From<Role> from = _database.From<Role>().RightJoin<UserRole>((r, ru) => r.Id == ru.RoleId);
            Where<Role> where = _database.Where<Role>().And<UserRole>(ru => ru.UserId == userId);

            return _database.RetrieveAsync<Role>(from, where, transContext)
                .ContinueWith(o=>o.Result.Select(r=>r.Name), TaskScheduler.Default);
        }

        public Task<int> GetRoleByNameAsync(string roleName, DbTransactionContext transContext = null)
        {
            //动用Cache
            return Task.FromResult(0);
        }
    }
}
