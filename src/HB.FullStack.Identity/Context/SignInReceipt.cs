using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Identity.Models;

using Microsoft.IdentityModel.Tokens;

namespace HB.FullStack.Identity
{
    public class SignInReceipt : ValidatableObject, IModel
    {
        public Guid UserId { get; set; }

        public string? Mobile { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public SignInReceipt() { }

        public SignInReceipt(string accessToken, string refreshToken, User user)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            UserId = user.Id;
            Email = user.Email;
            LoginName = user.Email;
            Mobile = user.Mobile;
            EmailConfirmed = user.EmailConfirmed;
            MobileConfirmed = user.MobileConfirmed;
            TwoFactorEnabled = user.TwoFactorEnabled;
            CreatedTime = DateTimeOffset.UtcNow;
        }

        public ModelKind GetKind() => ModelKind.Plain;
    }
}