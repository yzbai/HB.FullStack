﻿using System;
using System.Collections.Generic;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HB.FullStack.Server.Identity
{
    public class IdentityOptions : IOptions<IdentityOptions>
    {
        public IdentityOptions Value => this;

        public SignInSettings SignInSettings { get; set; } = new SignInSettings();

        public JwtSettings JwtSettings { get; set; } = new JwtSettings();
    }

    public class JwtSettings
    {
        public bool NeedAudienceToBeChecked { get; set; } = true;

        /// <summary>
        /// 接受json并验证的站点
        /// </summary>
        public IEnumerable<string>? ValidAudiences { get; set; }

        /// <summary>
        /// 签名算法
        /// </summary>
        public string SigningAlgorithm { get; set; } = SecurityAlgorithms.RsaSha256Signature;

        /// <summary>
        /// 用于签名的证书。签名让内容无法篡改，但可以被别人看到
        /// </summary>
        public string? JwtSigningCertificateSubject { get; set; }

        public string? JwtSigningCertificateFileName { get; set; }

        public string? JwtSigningCertificateFilePassword { get; set; }

        /// <summary>
        /// 用于加密Jwt内容证书。用于内容不被别人看到
        /// </summary>
        public string? JwtContentCertificateSubject { get; set; }

        public string? JwtContentCertificateFileName { get; set; }

        public string? JwtContentCertificateFilePassword { get; set; }

        public OpenIdConnectConfiguration OpenIdConnectConfiguration { get; set; } = new OpenIdConnectConfiguration();

        /// <summary>
        /// 连续两次请求Refresh最小时间间隔, 应该小于AccessToken的过期时间<see cref="SignInSettings.AccessTokenExpireTimeSpan"/>
        /// </summary>
        public TimeSpan RefreshIntervalTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

    }

    public class SignInSettings
    {
        public TimeSpan RefreshTokenLongExpireTimeSpan { get; set; } = TimeSpan.FromDays(365);
        public TimeSpan RefreshTokenShortExpireTimeSpan { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(5);

        #region LoginControl

        public TimeSpan LockoutTimeSpan { get; set; } = TimeSpan.FromHours(6);
        public bool RequiredMaxFailedCountCheck { get; set; } = true;
        public bool RequiredLockoutCheck { get; set; } = true;
        public bool RequireEmailConfirmed { get; set; }
        public bool RequireMobileConfirmed { get; set; }
        public bool RequireTwoFactorCheck { get; set; }
        public int MaxFailedCount { get; set; } = 4;
        public int AccessFailedRecoveryDays { get; set; } = 1;
        public int LockoutAfterAccessFailedCount { get; set; } = 4;

        #endregion

        public bool AllowRegisterByLoginName { get; set; }
    }

    //public class IdentityOptions : IOptions<IdentityOptions>
    //{
    //    public IdentityOptions Value { get { return this; } }

    //TODO: 考虑是否需要在SecurityStamp改变后，删除SignInCredential？
    //public IdentityEvents Events { get; set; } = new IdentityEvents();

    /// <summary>
    /// 用来查mobile，loginname，email是否重复的布隆表 名称
    /// </summary>
    //[Required]
    //public string BloomFilterName { get; set; } = null!;
    //}

    //public class IdentityEvents
    //{
    //    private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();

    //    public event AsyncEventHandler<SecurityStampChangedContext, EventArgs> SecurityStampChanged
    //    {
    //        add => _asyncEventManager.Add(value);
    //        remove => _asyncEventManager.Remove(value);
    //    }

    //    internal async Task OnSecurityStampChangedAsync(SecurityStampChangedContext context)
    //    {
    //        await _asyncEventManager.RaiseEventAsync(nameof(SecurityStampChanged), context, new EventArgs()).ConfigureAwait(false);
    //    }
    //}
}