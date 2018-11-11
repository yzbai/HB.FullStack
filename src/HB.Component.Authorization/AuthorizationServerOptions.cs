
using System;
using System.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace HB.Component.Authorization
{
    public class AuthorizationServerOptions : IOptions<AuthorizationServerOptions>
    {
        public AuthorizationServerOptions Value { get { return this; } }

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

    
}
