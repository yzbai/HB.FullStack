using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Identity.Models;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// 关联实体：UserClaim
    /// </summary>
    internal class CachedUserClaimsByUserId : CachedItem<IEnumerable<UserClaim>>
    {
        public CachedUserClaimsByUserId(Guid userId) : base(userId)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;

        public override string WhenToInvalidate => "UserClaim变动";
    }
}
