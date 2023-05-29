using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    /// <summary>
    /// 打包到Token里的信息，客户端不知
    /// </summary>
    public class UserClaim<TId> : DbModel<TId>, ITimestamp
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User<>), false)]
        public TId UserId { get; set; } = default!;

        [DbField(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [DbField(MaxLength = SharedNames.Length.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;
        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }

        //public bool AddToJwt { get; set; }
    }
}