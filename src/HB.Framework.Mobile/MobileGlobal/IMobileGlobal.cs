using System.Threading.Tasks;

namespace HB.Framework.Mobile
{
    public interface IMobileGlobal
    {
        #region Device

        Task<string> GetDeviceIdAsync();

        Task<string> GetDeviceTypeAsync();

        Task<string> GetDeviceVersionAsync();

        Task<string> GetDeviceAddressAsync();

        #endregion

        #region User

        Task<string> GetCurrentUserGuidAsync();

        Task SetCurrentUserGuidAsync(string userGuid);

        Task<bool> IsUserLoginedAsync();

        #endregion

        #region Token

        Task<string> GetAccessTokenAsync();

        Task SetAccessTokenAsync(string newAccessToken);

        Task<string> GetRefreshTokenAsync();

        Task SetRefreshTokenAsync(string refreshToken);

        #endregion
    }
}