using HB.Framework.Database.Entity;
using System;

namespace HB.Component.Identity.Entity
{
    [Serializable]
    public class UserClaim : DatabaseEntity
    {
        [DatabaseForeignKey("用户ID", typeof(User))]
        public long UserId { get; set; }

        [DatabaseEntityProperty("ClaimType", Length = 65530)]
        public string ClaimType { get; set; }

        [DatabaseEntityProperty("ClaimValue", Length = 65530)]
        public string ClaimValue { get; set; }
    }
}
