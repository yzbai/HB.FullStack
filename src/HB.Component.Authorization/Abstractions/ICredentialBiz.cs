using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ICredentialBiz
    {
        SigningCredentials SigningCredentials { get; }

        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        IEnumerable<SecurityKey> IssuerSigningKeys { get; }

        JsonWebKeySet JsonWebKeySet { get; }
        EncryptingCredentials EncryptingCredentials { get; }
        SecurityKey DecryptionSecurityKey { get; }
    }
}
