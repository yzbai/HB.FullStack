namespace HB.FullStack.Common.ApiClient
{
    public interface IUserPreferenceProvider
    {
        string? AccessToken { get; set; }
        string? RefreshToken { get; set; }
        string DeviceId { get; }
        string DeviceVersion { get; }
        bool IsLogined();

        void OnTokenRefreshFailed();
    }
}
