using HB.FullStack.Database.DBModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Models
{
    public class UserClaim : TimestampGuidDBModel
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [DatabaseModelProperty(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [DatabaseModelProperty(MaxLength = LengthConventions.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        public bool AddToJwt { get; set; }
    }
}