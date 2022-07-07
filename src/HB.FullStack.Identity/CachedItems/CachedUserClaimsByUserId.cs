using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using HB.FullStack.Identity.Models;
using HB.FullStack.Repository;

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
    }
}
