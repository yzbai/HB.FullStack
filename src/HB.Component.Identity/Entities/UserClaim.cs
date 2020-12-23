using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    public class UserClaim : IdEntity
    {
        [Required]
        [ForeignKey(typeof(User))]
        [EntityProperty(NotNull = true)]
        public long UserId { get; set; }

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        [EntityProperty]
        public bool AddToJwt { get; set; } = false;
    }
}
