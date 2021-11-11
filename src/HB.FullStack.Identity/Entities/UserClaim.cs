using HB.FullStack.Database.Entities;

using MessagePack;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    [MessagePackObject]
    public class UserClaim : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        [MessagePack.Key(7)]
        public Guid UserId { get; set; }

        [EntityProperty(NotNull = true)]
        [MessagePack.Key(8)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = LengthConventions.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        [MessagePack.Key(9)]
        public string ClaimValue { get; set; } = default!;

        [MessagePack.Key(10)]
        public bool AddToJwt { get; set; }
    }
}