using HB.Framework.AuthorizationServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using System.IO;
using HB.Framework.Common;

namespace HB.Framework.AuthorizationServer
{
    public class CredentialManager : ICredentialManager
    {
        private AuthorizationServerOptions _options;
        private IList<SecurityKey> _issuerSigningKeys;

        public CredentialManager(IOptions<AuthorizationServerOptions> options)
        {
            _options = options.Value;

        }

        public IEnumerable<SecurityKey> GetIssuerSigningKeys()
        {
            if (_issuerSigningKeys == null)
            {
                _issuerSigningKeys = new List<SecurityKey>();

                foreach (SecurityKey key in _options.JsonWebKeys.GetSigningKeys())
                {
                    _issuerSigningKeys.Add(key);
                }
            }

            return _issuerSigningKeys;
        }

        public SigningCredentials GetSigningCredentialsFromCertificate()
        {
            X509Certificate2 cert = new X509Certificate2(_options.CertificateFileName, _options.CertificatePassword, X509KeyStorageFlags.Exportable);
            X509SecurityKey securityKey = new X509SecurityKey(cert);
            return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256Signature);
        }
    }
}
