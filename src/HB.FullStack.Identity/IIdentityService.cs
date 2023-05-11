using System;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    public interface IIdentityService
    {
        #region Token

        string JsonWebKeySet { get; }

        Task<Token> RefreshTokenAsync(RefreshContext context, string lastUser);

        Task<Token> GetTokenAsync(SignInContext context, string lastUser);

        Task DeleteTokenAsync(Guid userId, DeviceIdiom idiom, SignInExclusivity logOffType, string lastUser);

        Task DeleteTokenAsync(Guid signInCredentialId, string lastUser);

        #endregion

        #region User

        Task RegisterUserAsync(RegisterContext context, string lastUser);

        #endregion

        #region UserProfile

        Task<UserProfile> GetUserProfileByUserIdAsync(Guid userId, string lastUser);

        Task UpdateUserProfileAsync(PropertyChangeJsonPack cp, string lastUser);

        #endregion

        #region Role

        //Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser);

        //Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser);

        #endregion

        #region UserActivity

        Task RecordUserActivityAsync(Guid? signInCredentialId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);
        

        #endregion
    }
}