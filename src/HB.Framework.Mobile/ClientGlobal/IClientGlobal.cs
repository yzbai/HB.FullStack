using System.Threading.Tasks;

namespace HB.Framework.Client
{
    public interface IClientGlobal
    {
        #region Device

        Task<string> GetDeviceIdAsync();

        Task<string> GetDeviceTypeAsync();

        Task<string> GetDeviceVersionAsync();

        Task<string> GetDeviceAddressAsync();

        #endregion

        #region User

        Task<string?> GetCurrentUserGuidAsync();

        Task SetCurrentUserGuidAsync(string? userGuid);

        Task<string?> GetCurrentLoginNameAsync();

        Task SetCurrentLoginNameAsync(string? loginName);

        Task<string?> GetCurrentMobileAsync();

        Task SetCurrentMobileAsync(string? mobile);

        Task<string?> GetCurrentEmailAsync();

        Task SetCurrentEmailAsync(string? email);

        Task<bool> IsUserLoginedAsync();

        #endregion

        #region Token

        Task<string?> GetAccessTokenAsync();

        Task SetAccessTokenAsync(string? newAccessToken);

        Task<string?> GetRefreshTokenAsync();

        Task SetRefreshTokenAsync(string? refreshToken);

        #endregion

        Task OnJwtRefreshSucceedAync(string newAccessToken);

        Task OnJwtRefreshFailedAync();

        Task OnLoginSuccessedAsync(string userGuid, string? loginName, string? mobile, string? email, string accessToken, string refreshToken);

        Task OnLoginFailedAsync();
    }
}