using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    public class UserClaim : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [EntityProperty(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = LengthConventions.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        public bool AddToJwt { get; set; }
    }
}