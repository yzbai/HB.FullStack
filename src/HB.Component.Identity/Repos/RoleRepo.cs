﻿using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    internal class RoleRepo : Repository<Role>
    {
        public RoleRepo(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
        }
    }
}
