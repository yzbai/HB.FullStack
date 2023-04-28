using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    public class UserClaim : TimestampGuidDbModel
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [DbField(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [DbField(MaxLength = SharedNames.Length.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        public bool AddToJwt { get; set; }
    }
}