using HB.Component.Identity.Entities;
using HB.FullStack.Business;
using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    /// <summary>
    /// userGuid: Roles
    /// </summary>
    internal class CachedRolesByUserGuid : CacheItem<IEnumerable<Role>>
    {
        private CachedRolesByUserGuid(params string[] keys) : base(keys)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;


        public static CachedRolesByUserGuid Key(string userGuid)
        {
            CachedRolesByUserGuid item = new CachedRolesByUserGuid(userGuid);

            return item;
        }
    }
}
