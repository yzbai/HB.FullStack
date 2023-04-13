using System;

namespace System
{
    /// <summary>
    /// 客户端主要信息Provider, 用于存储和获取客户端主要信息
    /// </summary>
    public interface IPreferenceProvider
    {
        Guid? UserId { get; set; }

        string? AccessToken { get; set; }

        string? RefreshToken { get; set; }

        string ClientId { get; }

        string ClientVersion { get; }

        DeviceInfos DeviceInfos { get; }

        bool IsIntroducedYet { get; set; }

        bool IsLogined();

        void OnLogined(Guid userId, DateTimeOffset userCreateTime, string? mobile, string? email, string? loginName, string accessToken, string refreshToken);

        void OnLogouted();

        void OnTokenRefreshFailed();
    }
}
