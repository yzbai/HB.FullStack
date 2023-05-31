using System;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    public interface IIdentityService<TId>
    {
        #region OpenIdConfiguration

        string OpenIdConnectConfigurationString { get; }

        string JsonWebKeySet { get; }
        #region Token

        Task<Token<TId>> RefreshTokenAsync(RefreshContext context, string lastUser);

        Task<Token<TId>> GetTokenAsync(SignInContext context, string lastUser);

        Task DeleteTokenAsync(TId userId, DeviceIdiom idiom, SignInExclusivity logOffType, string lastUser);

        Task DeleteTokenAsync(TId signInCredentialId, string lastUser);

        #endregion

        #region User

        Task RegisterUserAsync(RegisterContext context, string lastUser);

        #endregion

        #region UserProfile

        Task<UserProfile<TId>> GetUserProfileByUserIdAsync(TId userId, string lastUser);

        Task UpdateUserProfileAsync(PropertyChangePack cp, string lastUser);

        #endregion

        #region Role

        //Task AddRolesToUserAsync(TId userId, TId roleId, string lastUser);

        //Task RemoveRoleFromUserAsync(TId userId, TId roleId, string lastUser);

        #endregion

        #region UserActivity

        Task RecordUserActivityAsync(TId? signInCredentialId, TId? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);
        
        #endregion
    }
}