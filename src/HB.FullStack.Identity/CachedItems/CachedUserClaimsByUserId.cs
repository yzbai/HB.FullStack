using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// 关联实体：UserClaim
    /// </summary>
    public class CachedUserClaimsByUserId : CachedItem<IEnumerable<UserClaim>>
    {
        private CachedUserClaimsByUserId(params string[] keys) : base(keys)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;

        public static CachedUserClaimsByUserId Key(Guid userId)
        {
            CachedUserClaimsByUserId item = new CachedUserClaimsByUserId(userId.ToString());

            return item;
        }
    }
}
