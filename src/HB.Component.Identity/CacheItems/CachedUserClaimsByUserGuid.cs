﻿using System;
using System.Collections.Generic;
using System.Text;

using HB.Component.Identity.Entities;
using HB.FullStack.Business;

namespace HB.Component.Identity.CacheItems
{
    public class CachedUserClaimsByUserGuid : CacheItem<IEnumerable<UserClaim>>
    {
        private CachedUserClaimsByUserGuid(params string[] keys) : base(keys)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;

        public static CachedUserClaimsByUserGuid Key(string userGuid)
        {
            CachedUserClaimsByUserGuid item = new CachedUserClaimsByUserGuid(userGuid);

            return item;
        }
    }
}
