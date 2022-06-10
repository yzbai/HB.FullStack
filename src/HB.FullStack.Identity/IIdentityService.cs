using System;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    public interface IIdentityService
    {
        string JsonWebKeySetJson { get; }

        Task<UserAccessResult> RefreshAccessTokenAsync(RefreshContext context, string lastUser);

        Task<UserAccessResult> SignInAsync(SignInContext context, string lastUser);

        Task SignOutAsync(Guid userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser);

        Task SignOutAsync(Guid signInTokenId, string lastUser);

        Task OnSignInFailedBySmsAsync(string mobile, string lastUser);

        #region Role

        Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser);

        Task<bool> TryRemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser);

        #endregion

        #region UserActivity

        Task RecordUserActivityAsync(Guid? signInTokenId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);

        #endregion
    }
}