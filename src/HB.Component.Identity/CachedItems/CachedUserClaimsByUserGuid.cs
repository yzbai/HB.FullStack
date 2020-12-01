using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// 关联实体：UserClaim
    /// </summary>
    public class CachedUserClaimsByUserGuid : CachedItem<IEnumerable<UserClaim>>
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
