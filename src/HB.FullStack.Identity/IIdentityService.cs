using System;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
namespace HB.FullStack.Identity
{
    public interface IIdentityService
    {
        string JsonWebKeySetJson { get; }

        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        Task<UserAccessResult> RefreshAccessTokenAsync(RefreshContext context, string lastUser);

        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="KVStoreException"></exception>
        /// <exception cref="CacheException"></exception>
        Task<UserAccessResult> SignInAsync(SignInContext context, string lastUser);

        /// <exception cref="DatabaseException"></exception>
        Task SignOutAsync(Guid userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser);

        /// <exception cref="DatabaseException"></exception>
        Task SignOutAsync(Guid signInTokenId, string lastUser);

        /// <exception cref="KVStoreException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        Task OnSignInFailedBySmsAsync(string mobile, string lastUser);

        #region Role
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser);

        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser);

        #endregion

        #region UserActivity

        Task RecordUserActivityAsync(Guid? signInTokenId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);

        #endregion
    }
}