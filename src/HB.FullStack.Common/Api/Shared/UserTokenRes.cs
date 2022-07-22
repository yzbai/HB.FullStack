using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 可能存在多个Endpoint，即不同的Endpoint使用不同站点的UserToken
    /// </summary>
    public class UserTokenRes : ApiResource
    {
        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [Mobile]
        public string? Mobile { get; set; }

        [LoginName]
        public string? LoginName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;

        //protected override int GetChildHashCode()
        //{
        //    return HashCode.Combine(UserId, Mobile, LoginName, LoginName, Email, CreatedTime, AccessToken, RefreshToken);
        //}
    }
}