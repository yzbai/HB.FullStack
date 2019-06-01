using System.Security;
using System.Threading.Tasks;
using HB.Component.Authorization.Entity;
using HB.Component.Identity;
using HB.Component.Identity.Entity;

namespace HB.Component.Authorization.Abstractions
{
    internal interface IJwtBuilder
    {
        Task<string> BuildJwtAsync(User user, SignInToken signInToken, string audience);
        //string BuildJwt(User user, SignInToken signInToken, string audience);
    }
}
