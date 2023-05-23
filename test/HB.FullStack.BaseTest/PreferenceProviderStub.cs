using HB.FullStack.Client.Abstractions;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.BaseTest
{
    public class PreferenceProviderStub : ITokenPreferences
    {
        public PreferenceProviderStub()
        {
        }

        public Guid? UserId { get; set; }

        public string? UserLevel { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string ClientId { get; } = Guid.NewGuid().ToString();

        public string ClientVersion { get; } = "1.0";

        public DeviceInfos DeviceInfos { get; } = new DeviceInfos
        {
            Name = "Stub_Name",
            Model = "Stub_Model",
            OSVersion = "Stub_OSVersion",
            Platform = "Android",
            Type = "Stub_Type"
        };

        public bool IsLogined()
        {
            return false;
        }

        public void OnTokenRefreshFailed()
        {
        }

        public bool IsIntroducedYet { get; set; }

        public void OnLogined(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public void OnLogouted()
        {
            AccessToken = null;
            RefreshToken = null;
        }

        public string? Mobile { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public long? ExpiredAt { get; set; }

        public void OnTokenFetched(TokenRes tokenRes)
        {
            UserId = tokenRes.UserId;
            UserLevel = tokenRes.UserLevel;
            Mobile = tokenRes.Mobile;
            LoginName = tokenRes.LoginName;
            Email = tokenRes.Email;
            EmailConfirmed = tokenRes.EmailConfirmed;
            MobileConfirmed = tokenRes.MobileConfirmed;
            TwoFactorEnabled = tokenRes.TwoFactorEnabled;
            AccessToken = tokenRes.AccessToken ?? "";
            RefreshToken = tokenRes.RefreshToken ?? "";
            ExpiredAt = tokenRes.ExpiredAt;
        }

        public void OnTokenDeleted()
        {
            throw new NotImplementedException();
        }
    }
}