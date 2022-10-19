using System;

namespace HB.FullStack.Client.Maui
{
    public class MauiPreferenceProvider : IPreferenceProvider
    {
        public string? AccessToken { get => UserPreferences.AccessToken; set => UserPreferences.AccessToken = value ?? ""; }
        public string? RefreshToken { get => UserPreferences.RefreshToken; set => UserPreferences.RefreshToken = value ?? ""; }
        public string DeviceId { get => DevicePreferences.DeviceId; }
        public string DeviceVersion { get => DevicePreferences.DeviceVersion; }
        public DeviceInfos DeviceInfos { get => DevicePreferences.DeviceInfos; }

        public void OnTokenRefreshFailed() => UserPreferences.Logout();

        public bool IsLogined() => UserPreferences.IsLogined;

        public void OnTokenReceived(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken)
        {
            UserPreferences.Login(userId, userCreateTime, mobile, email, loginName, accessToken, refreshToken);
        }

        public Guid? UserId { get => UserPreferences.UserId; set => UserPreferences.UserId = value; }

    }
}
