using System;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
namespace HB.FullStack.Identity
{
    public interface IAuthorizationService
    {
        string JsonWebKeySetJson { get; }

        /// <exception cref="HB.FullStack.Identity.IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<UserAccessResult> RefreshAccessTokenAsync(RefreshContext context, string lastUser);

        /// <exception cref="HB.FullStack.Identity.IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.FullStack.KVStore.KVStoreException"></exception>
        Task<UserAccessResult> SignInAsync(SignInContext context, string lastUser);

        /// <exception cref="DatabaseException"></exception>
        Task SignOutAsync(long userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser);

        /// <exception cref="DatabaseException"></exception>
        Task SignOutAsync(long signInTokenId, string lastUser);

        /// <exception cref="HB.FullStack.KVStore.KVStoreException"></exception>
        Task OnSignInFailedBySmsAsync(string mobile, string lastUser);
    }
}