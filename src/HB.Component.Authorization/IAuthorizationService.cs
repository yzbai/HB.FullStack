using HB.Component.Authorization.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    public interface IAuthorizationService
    {
        Task<RefreshResult> RefreshAccessTokenAsync(RefreshContext context);
        Task<SignInResult> SignInAsync(SignInContext context);
        Task<AuthorizationResult> SignOutAsync(string signInTokenGuid);
        JsonWebKeySet GetJsonWebKeySet();
    }
}