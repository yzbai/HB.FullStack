using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace HB.Component.Authorization
{
    internal class CredentialBiz : ICredentialBiz
    {
        private readonly AuthorizationServerOptions _options;
        private readonly SigningCredentials _signingCredentials;
        private readonly EncryptingCredentials _encryptingCredentials;
        private readonly SecurityKey _decryptionSecurityKey;
        private readonly JsonWebKeySet _jsonWebKeySet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <exception cref="FileNotFoundException">证书文件不存在</exception>
        /// <exception cref="ArgumentException">Json无法解析</exception>
        public CredentialBiz(IOptions<AuthorizationServerOptions> options)
        {
            _options = options.Value;

            #region Signing Credentials 

            X509Certificate2? cert = CertificateUtil.GetBySubject(_options.SigningCertificateSubject);

            if (cert == null)
            {
                throw new FrameworkException(ErrorCode.JwtSigningCertNotFound, $"Subject:{_options.SigningCertificateSubject}");
            }

            X509SecurityKey securityKey = new X509SecurityKey(cert);

            _signingCredentials = new SigningCredentials(securityKey, _options.SigningAlgorithm.IsNullOrEmpty() ? SecurityAlgorithms.RsaSha256Signature : _options.SigningAlgorithm);

            #endregion

            #region JsonWebKeySet

            RSA publicKey = (RSA)securityKey.PublicKey;
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

            string jsonWebKeySetString = SerializeUtil.ToJson(new { Keys = jsonWebKeys });

            _jsonWebKeySet = new JsonWebKeySet(jsonWebKeySetString);

            #endregion

            #region Encryption Credentials
            X509Certificate2? encryptionCert = CertificateUtil.GetBySubject(_options.EncryptingCertificateSubject);

            if (encryptionCert == null)
            {
                throw new FrameworkException(ErrorCode.JwtEncryptionCertNotFound, $"Subject:{_options.EncryptingCertificateSubject}");
            }

            _encryptingCredentials = new X509EncryptingCredentials(encryptionCert);

            #endregion

            #region Decryption Security Key

            _decryptionSecurityKey = new X509SecurityKey(encryptionCert);

            #endregion
        }

        /// <summary>
        /// 公钥
        /// </summary>
        /// <returns></returns>
        public JsonWebKeySet JsonWebKeySet => _jsonWebKeySet;

        /// <summary>
        /// GetIssuerSigningKeys
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        public IEnumerable<SecurityKey> IssuerSigningKeys
        {
            get
            {
                if (_jsonWebKeySet == null)
                {
                    throw new AuthorizationException(Resources.JsonWebKeySetIsNullErrorMessage);
                }

                return _jsonWebKeySet.GetSigningKeys();
            }
        }

        public SigningCredentials SigningCredentials => _signingCredentials;

        public EncryptingCredentials EncryptingCredentials => _encryptingCredentials;

        public SecurityKey DecryptionSecurityKey => _decryptionSecurityKey;
    }
}
