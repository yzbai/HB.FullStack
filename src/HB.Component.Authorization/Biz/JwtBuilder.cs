using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entities;
using HB.Component.Identity;
using HB.Component.Identity.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class JwtBuilder : IJwtBuilder
    {
        private readonly SignInOptions _signInOptions;
        private readonly AuthorizationServerOptions _options;
        private readonly SigningCredentials _signingCredentials;
        private readonly EncryptingCredentials _encryptingCredentials;

        private readonly IIdentityService _identityService;

        public JwtBuilder(IOptions<AuthorizationServerOptions> options, ICredentialBiz credentialBiz, IIdentityService identityService)
        {
            _options = options.Value;
            _signInOptions = _options.SignInOptions;
            _signingCredentials = credentialBiz.SigningCredentials;
            _encryptingCredentials = credentialBiz.EncryptingCredentials;
            _identityService = identityService;
        }

        public async Task<string> BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(IdentityUser user, SignInToken signInToken, string? audience)
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new()
        {
            DateTime utcNow = DateTime.UtcNow;

            IEnumerable<Claim> userClaims = await _identityService.GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(user).ConfigureAwait(false);
            
            IList<Claim> claims = userClaims.ToList();

            claims.Add(new Claim(ClaimExtensionTypes.SignInTokenGuid, signInToken.Guid));

            //这个JWT只能在当前DeviceId上使用
            claims.Add(new Claim(ClaimExtensionTypes.DeviceId, signInToken.DeviceId));

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            JwtSecurityToken token = handler.CreateJwtSecurityToken(
                _options.OpenIdConnectConfiguration.Issuer,
                _options.NeedAudienceToBeChecked ? audience : null,
                new ClaimsIdentity(claims),
                utcNow,
                utcNow + _signInOptions.AccessTokenExpireTimeSpan,
                utcNow,
                _signingCredentials,
                _encryptingCredentials
                );

            return handler.WriteToken(token);
        }
    }
}
