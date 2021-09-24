using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class UserActivityRepo : DbEntityRepository<UserActivity>
    {
        public UserActivityRepo(ILogger<UserActivityRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {

        }
    }
}
