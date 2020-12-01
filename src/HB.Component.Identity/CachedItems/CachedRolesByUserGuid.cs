using HB.FullStack.Identity.Entities;
using HB.FullStack.Business;
using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// userGuid: Roles
    /// 关联实体：RoleOfUser
    /// </summary>
    internal class CachedRolesByUserGuid : CachedItem<IEnumerable<Role>>
    {
        private CachedRolesByUserGuid(params string[] keys) : base(keys)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;


        public static CachedRolesByUserGuid Key(string userGuid)
        {
            return new CachedRolesByUserGuid(userGuid);
        }
    }
}
