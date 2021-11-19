using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HB.FullStack.Identity
{
    public static class JwtHelper
    {
        public static string BuildJwt(
            IEnumerable<Claim> claims,
            string issuer,
            string? audience,
            TimeSpan accessTokenExpiryTime,
            SigningCredentials signingCredentials,
            EncryptingCredentials encryptingCredentials)
        {
            DateTime utcNow = DateTime.UtcNow;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            JwtSecurityToken token = handler.CreateJwtSecurityToken(
                issuer,
                audience,
                new ClaimsIdentity(claims),
                utcNow,
                utcNow + accessTokenExpiryTime,
                utcNow,
                signingCredentials,
                encryptingCredentials
                );

            return handler.WriteToken(token);
        }

        public static ClaimsPrincipal ValidateTokenWithoutLifeCheck(
            string tokenToValidate,
            string issuer,
            bool validateAudience,
            IEnumerable<string>? validAudiences,
            IEnumerable<SecurityKey> issuerSigningKeys,
            SecurityKey decryptionSecurityKey)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidAudiences = validAudiences,
                ValidateAudience = validateAudience,
#pragma warning disable CA5404 // 这里是为了刷新token而验证
                ValidateLifetime = false,
#pragma warning restore CA5404 // Do not disable token validation checks
                ValidIssuer = issuer,
                IssuerSigningKeys = issuerSigningKeys,
                TokenDecryptionKey = decryptionSecurityKey
            };

            return new JwtSecurityTokenHandler().ValidateToken(tokenToValidate, parameters, out _);
        }
    }
}