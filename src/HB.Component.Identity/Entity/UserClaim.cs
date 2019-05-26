using HB.Framework.Database.Entity;
using System;

namespace HB.Component.Identity.Entity
{
    public class UserClaim : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [ForeignKey(typeof(User))]
        [GuidEntityProperty]
        public string UserGuid { get; set; }

        [EntityProperty("ClaimType", Length = 65530)]
        public string ClaimType { get; set; }

        [EntityProperty("ClaimValue", Length = 65530)]
        public string ClaimValue { get; set; }

        [EntityProperty]
        public bool AddToJwt { get; set; } = false;
    }
}
