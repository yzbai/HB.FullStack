using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using System.IO;
using HB.Framework.Common;
using HB.Component.Authorization.Abstractions;
using System.Security.Cryptography;
using System.Security;

namespace HB.Component.Authorization
{
    public class CredentialManager : ICredentialManager
    {
        private AuthorizationServerOptions _options;
        private SigningCredentials _signingCredentials;
        private JsonWebKeySet _jsonWebKeySet;

        public CredentialManager(IOptions<AuthorizationServerOptions> options)
        {
            _options = options.Value;

            X509Certificate2 cert = CertificateUtil.GetBySubject(_options.CertificateSubject, StoreLocation.CurrentUser, StoreName.My);
            X509SecurityKey securityKey = new X509SecurityKey(cert);

            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256Signature);
                        
            RSA publicKey = securityKey.PublicKey as RSA;
            RSAParameters parameters = publicKey.ExportParameters(false);

            IList<JsonWebKey> jsonWebKeys = new List<JsonWebKey> {
                new JsonWebKey {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = securityKey.KeyId,
                    E = Base64UrlEncoder.Encode(parameters.Exponent),
                    N = Base64UrlEncoder.Encode(parameters.Modulus)
                }
            };

            string jsonString = DataConverter.ToJson(new { Keys = jsonWebKeys });

            _jsonWebKeySet = new JsonWebKeySet(jsonString);
        }

        /// <summary>
        /// 公钥
        /// </summary>
        /// <returns></returns>
        public JsonWebKeySet GetJsonWebKeySet()
        {
            return _jsonWebKeySet;
        }

        public IEnumerable<SecurityKey> GetIssuerSigningKeys()
        {
            if (_jsonWebKeySet == null)
            {
                return null;
            }

            return _jsonWebKeySet.GetSigningKeys();
        }

        /// <summary>
        /// 私钥
        /// </summary>
        /// <returns></returns>
        public SigningCredentials GetSigningCredentials()
        {
            return _signingCredentials;
        }
    }
}
