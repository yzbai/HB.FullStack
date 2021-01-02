
using System;
using System.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HB.Component.Authorization
{
    public class AuthorizationServerOptions : IOptions<AuthorizationServerOptions>
    {
        public AuthorizationServerOptions Value { get { return this; } }

        /// <summary>
        /// 面向用户的要求
        /// </summary>
        public SignInOptions SignInOptions { get; set; } = new SignInOptions();

        #region Jwt Options;

        public bool NeedAudienceToBeChecked { get; set; } = true;

        /// <summary>
        /// 签名算法
        /// </summary>
        public string SigningAlgorithm { get; set; } = SecurityAlgorithms.RsaSha256Signature;

        /// <summary>
        /// 用于签名的证书。签名让内容无法篡改，但可以被别人看到
        /// </summary>
        [DisallowNull, NotNull]
        public string? SigningCertificateSubject { get; set; }


        //TODO: 在appsettings.json中暂时用了DataProtection的证书，正式发布时需要换掉
        /// <summary>
        /// 用于加密的证书。用于内容不被别人看到
        /// </summary>
        [DisallowNull, NotNull]
        public string? EncryptingCertificateSubject { get; set; }

        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; } = new OpenIdConnectConfiguration();


        /// <summary>
        /// 连续两次请求Refresh最小时间间隔
        /// </summary>
        public TimeSpan RefreshIntervalTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

        #endregion
    }

    public class SignInOptions
    {
        public TimeSpan RefreshTokenLongExpireTimeSpan { get; set; } = TimeSpan.FromDays(365);
        public TimeSpan RefreshTokenShortExpireTimeSpan { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LockoutTimeSpan { get; set; } = TimeSpan.FromHours(6);
        public bool RequiredMaxFailedCountCheck { get; set; }
        public bool RequiredLockoutCheck { get; set; }
        public bool RequireEmailConfirmed { get; set; }
        public bool RequireMobileConfirmed { get; set; }
        public bool RequireTwoFactorCheck { get; set; }
        public long MaxFailedCount { get; set; } = 4;
        public double AccessFailedRecoveryDays { get; set; } = 1;
        public long LockoutAfterAccessFailedCount { get; set; } = 4;
        public bool AllowOnlyOneAppClient { get; set; } = true;
    }


}
