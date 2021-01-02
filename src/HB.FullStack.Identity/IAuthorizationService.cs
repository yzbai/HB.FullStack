using System;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
namespace HB.FullStack.Identity
{
    public interface IAuthorizationService
    {
        string JsonWebKeySetJson { get; }

        Task<UserAccessResult> RefreshAccessTokenAsync(RefreshContext context, string lastUser);

        Task<UserAccessResult> SignInAsync(SignInContext context, string lastUser);

        Task SignOutAsync(long userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser);

        Task SignOutAsync(long signInTokenId, string lastUser);

        Task OnSignInFailedBySmsAsync(string mobile, string lastUser);
    }
}