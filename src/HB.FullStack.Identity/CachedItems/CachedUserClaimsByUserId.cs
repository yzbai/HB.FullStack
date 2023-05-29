using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    /// <summary>
    /// 关联实体：UserClaim
    /// </summary>
    internal class CachedUserClaimsByUserId<TId> : CachedItem<IList<UserClaim<TId>>>
    {
        public CachedUserClaimsByUserId(TId userId) : base(userId)
        {
        }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => null;

        public override string WhenToInvalidate => "UserClaim变动";
    }
}
