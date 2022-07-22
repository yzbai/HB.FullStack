using System;

namespace System
{
    /// <summary>
    /// 客户端主要信息Provider
    /// </summary>
    public interface IPreferenceProvider
    {
        Guid? UserId { get; set; }

        string? AccessToken { get; set; }

        string? RefreshToken { get; set; }

        string DeviceId { get; }

        string DeviceVersion { get; }

        DeviceInfos DeviceInfos { get; }

        bool IsLogined();

        void OnTokenReceived(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken);

        void OnTokenRefreshFailed();
    }
}
