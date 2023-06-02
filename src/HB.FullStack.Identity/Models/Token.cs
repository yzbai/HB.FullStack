/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Server.Identity.Models
{
    public class Token<TId> : ValidatableObject, IModel, IExpired
    {
        public TId UserId { get; set; } = default!;

        public string? UserLevel { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? Mobile { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public long? ExpiredAt { get; set; }

        public Token() { }

        public Token(string accessToken, string refreshToken, User<TId> user, long expiredAt)
        {
            UserId = user.Id;
            UserLevel = user.UserLevel;
            LoginName = user.Email;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            Mobile = user.Mobile;
            MobileConfirmed = user.MobileConfirmed;
            TwoFactorEnabled = user.TwoFactorEnabled;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiredAt = expiredAt;
        }

        public ModelKind GetKind() => ModelKind.Plain;

    }
}