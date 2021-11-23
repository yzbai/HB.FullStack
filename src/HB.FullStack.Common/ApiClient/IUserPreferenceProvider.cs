using System;

namespace HB.FullStack.Common.ApiClient
{
    public interface IPreferenceProvider
    {
        Guid? UserId { get; set; }
        string? AccessToken { get; set; }
        string? RefreshToken { get; set; }
        string DeviceId { get; }
        string DeviceVersion { get; }
        bool IsLogined();

        void OnTokenRefreshFailed();
    }
}
