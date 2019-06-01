
using System;
using System.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace HB.Component.Authorization
{
    public class AuthorizationOptions : IOptions<AuthorizationOptions>
    {
        public AuthorizationOptions Value { get { return this; } }

        public SignInOptions SignInOptions { get; set; } = new SignInOptions();

        public bool NeedAudienceToBeChecked { get; set; } = true;

        public string CertificateSubject { get; set; }

        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; }

        //public JsonWebKeySet JsonWebKeys { get; set; }

        /// <summary>
        /// 连续两次请求Refresh最小时间间隔
        /// </summary>
        public TimeSpan RefreshIntervalTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

    }

    public class SignInOptions
    {
        public TimeSpan RefreshTokenLongExpireTimeSpan { get; set; } = TimeSpan.FromDays(365);
        public TimeSpan RefreshTokenShortExpireTimeSpan { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan LockoutTimeSpan { get; set; } = TimeSpan.FromHours(6);
        public bool RequiredMaxFailedCountCheck { get; set; } = false;
        public bool RequiredLockoutCheck { get; set; } = false;
        public bool RequireEmailConfirmed { get; set; } = false;
        public bool RequireMobileConfirmed { get; set; } = false;
        public bool RequireTwoFactorCheck { get; set; } = false;
        public long MaxFailedCount { get; set; } = 4;
        public double AccessFailedRecoveryDays { get; set; } = 1;
        public long LockoutAfterAccessFailedCount { get; set; } = 4;
        public bool AllowOnlyOneAppClient { get; set; } = true;
    }


}
