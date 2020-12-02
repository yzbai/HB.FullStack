using System;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

namespace HB.FullStack.Identity
{
    public interface IAuthorizationService
    {
        JsonWebKeySet JsonWebKeySet { get; }

        Task<string> RefreshAccessTokenAsync(RefreshContext context, string lastUser);
        Task<SignInResult> SignInAsync(SignInContext context, string lastUser);
        Task SignOutAsync(string userGuid, DeviceIdiom idiom, LogOffType logOffType, string lastUser);
        Task SignOutAsync(string signInTokenGuid, string lastUser);

        Task OnSignInFailedBySmsAsync(string mobile, string lastUser);
    }
}