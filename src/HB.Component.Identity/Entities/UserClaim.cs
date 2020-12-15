using HB.FullStack.Common.Entities;

using System;

namespace HB.FullStack.Identity.Entities
{
    [DatabaseEntity]
    public class UserClaim : Entity
    {
        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        [EntityProperty]
        public bool AddToJwt { get; set; } = false;
    }
}
