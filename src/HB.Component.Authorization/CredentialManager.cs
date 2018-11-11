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
    [SecurityCritical]
    public class CredentialManager : ICredentialManager
    {
        private AuthorizationServerOptions _options;
        private SigningCredentials _signingCredentials;
        private JsonWebKeySet _jsonWebKeySet;

        [SecurityCritical]
        public CredentialManager(IOptions<AuthorizationServerOptions> options)
        {
            _options = options.Value;

            using (X509Certificate2 cert = CertificateUtil.GetBySubject(_options.CertificateSubject, StoreLocation.LocalMachine, StoreName.Root))
            {

                RSA privateKey = cert.PrivateKey as RSA;

                

                //X509Certificate2 cert2 = new X509Certificate2("Brlite_AHabit_JWT_Development.pfx","Brlite2015");

                X509SecurityKey securityKey = new X509SecurityKey(cert);


                SecurityKey key = securityKey;

                var factory = key.CryptoProviderFactory;

                byte[] data = Encoding.UTF8.GetBytes("Helellel");

                byte[] encrypData = key.CryptoProviderFactory.CreateForSigning(key, SecurityAlgorithms.RsaSha256Signature).Sign(data);

                string rawStr = Base64UrlEncoder.Encode(encrypData);

                if (key is AsymmetricSecurityKey asymmetricSecurityKey)
                {
                    PrivateKeyStatus status = asymmetricSecurityKey.PrivateKeyStatus;
                }

                if (key is JsonWebKey jsonWebKey)
                {
                    PrivateKeyStatus status = jsonWebKey.HasPrivateKey ? PrivateKeyStatus.Exists : PrivateKeyStatus.DoesNotExist;
                }


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

        }

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

        public SigningCredentials GetSigningCredentialsFromCertificate()
        {
            return _signingCredentials;
        }
    }
}
