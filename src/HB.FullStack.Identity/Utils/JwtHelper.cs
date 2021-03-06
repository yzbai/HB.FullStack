﻿using Microsoft.IdentityModel.Tokens;

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

        public static ClaimsPrincipal ValidateTokenWithoutLifeCheck(string tokenToValidate, string issuer, IEnumerable<SecurityKey> issuerSigningKeys, SecurityKey decryptionSecurityKey)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidIssuer = issuer,
                IssuerSigningKeys = issuerSigningKeys,
                TokenDecryptionKey = decryptionSecurityKey
            };

            return new JwtSecurityTokenHandler().ValidateToken(tokenToValidate, parameters, out _);
        }
    }
}
