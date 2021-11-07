using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// userid: Roles
    /// 关联实体：RoleOfUser
    /// </summary>
    internal class CachedUserRolesByUserId : CachedItem<IEnumerable<Role>>
    {
        private CachedUserRolesByUserId(params string[] keys) : base(keys)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;


        public static CachedUserRolesByUserId Key(Guid userId)
        {
            return new CachedUserRolesByUserId(userId.ToString());
        }
    }
}
