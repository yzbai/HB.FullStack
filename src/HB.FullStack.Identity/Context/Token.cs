/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    public class Token : ValidatableObject, IModel
    {
        public Guid UserId { get; set; }

        public string? UserLevel { get; set; }

        public string? Mobile { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTimeOffset TokenCreatedTime { get; set; }

        public Token()
        { }

        public Token(string accessToken, string refreshToken, User user)
        {
            UserLevel = user.UserLevel;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            UserId = user.Id;
            Email = user.Email;
            LoginName = user.Email;
            Mobile = user.Mobile;
            EmailConfirmed = user.EmailConfirmed;
            MobileConfirmed = user.MobileConfirmed;
            TwoFactorEnabled = user.TwoFactorEnabled;
            TokenCreatedTime = DateTimeOffset.UtcNow;
        }

        public ModelKind GetKind() => ModelKind.Plain;
    }
}