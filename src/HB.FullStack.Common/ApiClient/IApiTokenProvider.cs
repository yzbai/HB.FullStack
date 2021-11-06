namespace HB.FullStack.Common.ApiClient
{
    public interface IApiTokenProvider
    {
        string? AccessToken { get; set; }
        string? RefreshToken { get; set; }
        string DeviceId { get; }
        string DeviceVersion { get; }

        void OnTokenRefreshFailed();
    }
}
