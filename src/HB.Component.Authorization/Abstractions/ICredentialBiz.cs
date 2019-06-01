using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ICredentialBiz
    {
        SigningCredentials GetSigningCredentials();

        IEnumerable<SecurityKey> GetIssuerSigningKeys();

        JsonWebKeySet GetJsonWebKeySet();
    }
}
