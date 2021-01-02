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
    internal class CredentialHelper
    {
        private readonly AuthorizationServerOptions _options;

        //公钥集
        private readonly JsonWebKeySet _jsonWebKeySet;

        //签名凭证（证书+签名算法）
        private readonly SigningCredentials _signingCredentials;

        //加密凭证(证书+签名算法）
        private readonly EncryptingCredentials _encryptingCredentials;

        /// <summary>
        ///解密私钥
        /// </summary>
        private readonly SecurityKey _decryptionSecurityKey;

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

            //证书
            X509Certificate2? cert = CertificateUtil.GetBySubject(_options.SigningCertificateSubject);

            if (cert == null)
            {
                throw new FrameworkException(ErrorCode.JwtSigningCertNotFound, $"Subject:{_options.SigningCertificateSubject}");
            }

            //密钥
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
        /// 从证书获得密钥,可以用来解密
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static SecurityKey GetSecurityKey(X509Certificate2 cert)
        {
            return new X509SecurityKey(cert);
        }

        /// <summary>
        /// 从证书获得加密凭证
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static EncryptingCredentials GetEncryptingCredentials(X509Certificate2 cert)
        {
            return new X509EncryptingCredentials(cert);
        }

        /// <summary>
        /// 从证书获取签名凭证
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="signingAlgorithm"></param>
        /// <returns></returns>
        public static SigningCredentials GetSigningCredentials(X509Certificate2 cert, string? signingAlgorithm = null)
        {
            SecurityKey securityKey = GetSecurityKey(cert);

            return new SigningCredentials(securityKey, signingAlgorithm ?? SecurityAlgorithms.RsaSha256Signature);
        }

        /// <summary>
        /// 从证书获取JsonWebKeySet（公钥集合）
        /// </summary>
        /// <returns></returns>
        public static JsonWebKeySet CreateJsonWebKeySet(X509Certificate2 cert)
        {
            //密钥
            X509SecurityKey securityKey = new X509SecurityKey(cert);

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

            return new JsonWebKeySet(jsonWebKeySetString);
        }

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
