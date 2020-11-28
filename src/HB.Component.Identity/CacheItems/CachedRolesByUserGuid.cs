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
    internal class CachedRolesByUserGuid : CacheItem<CachedRolesByUserGuid, IEnumerable<Role>>
    {
        //private const string _prefix = "Role";

        //private readonly string _userGuid;

        //public RolesByUserGuidCacheItem(string userGuid)
        //{
        //    _userGuid = userGuid;
        //}


        //public override string CacheKey => _prefix + _userGuid;


        //public static RolesByUserGuidCacheItem Key(string userGuid)
        //{
        //    return new RolesByUserGuidCacheItem(userGuid);
        //}


        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;

        public override string Prefix => "Role";
    }
}
