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
    public static class JwtHelper
    {
        public static string BuildJwt(
            IEnumerable<Claim> claims,
            SignInToken signInToken,
            string issuer,
            string? audience,
            TimeSpan accessTokenExpiryTime,
            SigningCredentials signingCredentials,
            EncryptingCredentials encryptingCredentials)
        {
            DateTime utcNow = DateTime.UtcNow;

            IList<Claim> claimList = claims.ToList();

            claimList.Add(new Claim(ClaimExtensionTypes.SignInTokenGuid, signInToken.Guid));

            //这个JWT只能在当前DeviceId上使用
            claimList.Add(new Claim(ClaimExtensionTypes.DeviceId, signInToken.DeviceId));

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            //JwtSecurityToken token = handler.CreateJwtSecurityToken(
            //    _options.OpenIdConnectConfiguration.Issuer,
            //    _options.NeedAudienceToBeChecked ? audience : null,
            //    new ClaimsIdentity(claims),
            //    utcNow,
            //    utcNow + _signInOptions.AccessTokenExpireTimeSpan,
            //    utcNow,
            //    _signingCredentials,
            //    _encryptingCredentials
            //    );

            JwtSecurityToken token = handler.CreateJwtSecurityToken(
                issuer,
                audience, // null if _options.NeedAudienceToBeChecked is false
                new ClaimsIdentity(claims),
                utcNow,
                utcNow + accessTokenExpiryTime,
                utcNow,
                signingCredentials,
                encryptingCredentials
                );

            return handler.WriteToken(token);
        }
    }
}
