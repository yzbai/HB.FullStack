using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    public interface IUserClaim
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
        object UserId { get; set; }
    }

    /// <summary>
    /// 打包到Token里的信息，客户端不知
    /// </summary>
    public class UserClaim : TimestampGuidDbModel, IUserClaim
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [DbField(NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [DbField(MaxLength = SharedNames.Length.MAX_USER_CLAIM_VALUE_LENGTH, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        //public bool AddToJwt { get; set; }
    }
}