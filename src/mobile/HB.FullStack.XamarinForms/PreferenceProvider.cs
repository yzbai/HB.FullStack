using HB.FullStack.Common.ApiClient;
using HB.FullStack.XamarinForms.Api;

using System;

namespace HB.FullStack.XamarinForms
{
    public class PreferenceProvider : IPreferenceProvider
    {
        public string? AccessToken { get => UserPreferences.AccessToken; set => UserPreferences.AccessToken = value ?? ""; }
        public string? RefreshToken { get => UserPreferences.RefreshToken; set => UserPreferences.RefreshToken = value ?? ""; }
        public string DeviceId { get => DevicePreferences.DeviceId; }
        public string DeviceVersion { get => DevicePreferences.DeviceVersion; }

        public void OnTokenRefreshFailed() => UserPreferences.Logout();

        public bool IsLogined() => UserPreferences.IsLogined;

        public Guid? UserId { get => UserPreferences.UserId; set => UserPreferences.UserId = value; }
    }
}
