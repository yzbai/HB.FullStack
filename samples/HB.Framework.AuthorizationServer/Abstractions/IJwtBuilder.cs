using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Identity;
using System.Threading.Tasks;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public interface IJwtBuilder
    {
        Task<string> BuildJwtAsync(User user, SignInToken signInToken, string audience);
    }
}
