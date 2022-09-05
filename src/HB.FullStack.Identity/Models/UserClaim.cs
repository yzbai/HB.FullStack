using HB.FullStack.Database.DbModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Models
{
    public class UserClaim : TimestampGuidDbModel
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [DBModelProperty(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [DBModelProperty(MaxLength = LengthConventions.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        public bool AddToJwt { get; set; }
    }
}