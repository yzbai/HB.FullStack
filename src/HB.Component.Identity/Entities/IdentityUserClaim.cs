using HB.Framework.Common.Entities;
using HB.Framework.Database.Entities;
using System;

namespace HB.Component.Identity.Entities
{
    public abstract class IdentityUserClaim : Entity
    {
        [ForeignKey(typeof(IdentityUser))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [EntityProperty("ClaimType", Length = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty("ClaimValue", Length = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        [EntityProperty]
        public bool AddToJwt { get; set; } = false;
    }
}
