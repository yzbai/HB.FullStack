using HB.FullStack.Common.Shared;

namespace Todo.Client.ConsoleApp
{
    class ConsolePreferenceProvider : IPreferenceProvider
    {
        public Guid? UserId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string ClientId { get; } = "ClientId123";

        public string ClientVersion { get; } = "ClientVersion1";

        public DeviceInfos DeviceInfos { get; } = new DeviceInfos { Name = "DName", Model = "DModel", Idiom = DeviceIdiom.Web, OSVersion = "DOS", Platform = "Win", Type = "sf" };

        public bool IsIntroducedYet { get; set; }

        public bool IsLogined()
        {
            return AccessToken != null && RefreshToken != null;
        }

        public void OnLogined(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken)
        {
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

        }

        public void OnLogouted()
        {
            AccessToken = null;
            RefreshToken = null;
        }

        public void OnTokenRefreshFailed()
        {
            throw new NotImplementedException();
        }
    }
}