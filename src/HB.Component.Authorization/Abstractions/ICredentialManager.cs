using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Authorization.Abstractions
{
    public interface ICredentialManager
    {
        SigningCredentials GetSigningCredentialsFromCertificate();

        IEnumerable<SecurityKey> GetIssuerSigningKeys();

        JsonWebKeySet GetJsonWebKeySet();
    }
}
