using System;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Context;

namespace HB.FullStack.Server.Identity
{
    public interface IIdentityService
    {
        string JsonWebKeySet { get; }

        Task<SignInReceipt> RefreshSignInReceiptAsync(RefreshContext context, string lastUser);

        Task<SignInReceipt> SignInAsync(SignInContext context, string lastUser);

        Task SignOutAsync(Guid userId, DeviceIdiom idiom, SignInExclusivity logOffType, string lastUser);

        Task SignOutAsync(Guid signInCredentialId, string lastUser);

        Task RegisterAsync(RegisterContext context, string lastUser);

        #region Role

        //Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser);

        //Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser);

        #endregion

        #region UserActivity

        Task RecordUserActivityAsync(Guid? signInCredentialId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);

        #endregion
    }
}