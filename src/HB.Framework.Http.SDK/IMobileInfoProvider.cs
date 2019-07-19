using System.Threading.Tasks;

namespace HB.Framework.Http.SDK
{
    public interface IMobileInfoProvider
    {
        Task<string> GetDeviceIdAsync();
        Task<string> GetDeviceTypeAsync();
        Task<string> GetDeviceVersionAsync();

        Task<string> GetDeviceAddressAsync();

        Task<string> GetAccessTokenAsync();

        Task<string> GetRefreshTokenAsync();

        Task SetAccessTokenAsync(string accessToken);

        Task SetRefreshTokenAsync(string refreshToken);
    }
}
