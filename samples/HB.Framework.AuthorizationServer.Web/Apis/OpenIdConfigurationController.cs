using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using HB.Framework.Common;
using Newtonsoft.Json;

namespace HB.Framework.AuthorizationServer.Web.Apis
{
    //TODO: 增添对Audience的验证，比如IP验证
    [AllowAnonymous]
    [Route(".well-known/openid-configuration")]
    public class OpenIdConfigurationController : Controller
    {
        private AuthorizationServerOptions _options;
        private static string _openIdConnectConfigurationString;
        private static string _jwkString;

        public OpenIdConfigurationController(IOptions<AuthorizationServerOptions> options)
        {
            _options = options.Value;

            _openIdConnectConfigurationString = JsonConvert.SerializeObject(_options.OpenIdConnectConfiguration);
            _jwkString = JsonConvert.SerializeObject(_options.JsonWebKeys);
        }

        
        [HttpGet]
        public string OpenIdConnectConfiguration()
        {
            return _openIdConnectConfigurationString;
        }

        [HttpGet("Jwks")]
        public string Jwks()
        {
            return _jwkString;
        }

    }
}
