using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HB.FullStack.Server.Identity
{
    public static class JwtHelper
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        /// <summary>
        /// Create Jwt.
        /// </summary>
        /// <param name="claims">Contet used to create jwt.</param>
        /// <param name="issuer">Issuer.</param>
        /// <param name="audience">Audience.</param>
        /// <param name="tokenExpiryTime">Jwt Expiry Time.</param>
        /// <param name="signingCredentials">Credentials Used for Signing.</param>
        /// <param name="contentEncryptingCredentials">Credentials Used for Content Encrypting.</param>
        /// <returns></returns>
        public static string CreateJwt(
            IEnumerable<Claim> claims,
            string issuer,
            string? audience,
            TimeSpan tokenExpiryTime,
            SigningCredentials signingCredentials,
            EncryptingCredentials contentEncryptingCredentials)
        {
            DateTime utcNow = DateTime.UtcNow;

            JwtSecurityToken token = _tokenHandler.CreateJwtSecurityToken(
                issuer,
                audience,
                new ClaimsIdentity(claims),
                utcNow,
                utcNow + tokenExpiryTime,
                utcNow,
                signingCredentials,
                contentEncryptingCredentials);

            return _tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate Jwt without Lifetime Check.
        /// </summary>
        /// <param name="tokenToValidate">jwt string that are going to validate</param>
        /// <param name="issuer">Who publish this jwt</param>
        /// <param name="validateAudience">Whether validate Audience</param>
        /// <param name="validAudiences">Who are correct Audience</param>
        /// <param name="issuerSigningKeys">Used for Signature Validation</param>
        /// <param name="contentDecryptionSecurityKey">Security Key Used for Content Decryption</param>
        /// <returns></returns>
        public static ClaimsPrincipal ValidateTokenWithoutLifeCheck(
            string tokenToValidate,
            string issuer,
            bool validateAudience,
            IEnumerable<string>? validAudiences,
            IEnumerable<SecurityKey> issuerSigningKeys,
            SecurityKey contentDecryptionSecurityKey)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidAudiences = validAudiences,
                ValidateAudience = validateAudience,
                ValidateLifetime = false,       //Without Lifetime check
                ValidIssuer = issuer,
                IssuerSigningKeys = issuerSigningKeys,
                TokenDecryptionKey = contentDecryptionSecurityKey
            };

            return _tokenHandler.ValidateToken(tokenToValidate, parameters, out _);
        }
    }
}