/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.Abstractions
{
    public interface ITokenPreferences : IExpired
    {
        #region Client

        string ClientId { get; }

        string ClientVersion { get; }

        DeviceInfos DeviceInfos { get; }

        #endregion

        Guid? UserId { get; }

        string? UserLevel { get; }

        string? Mobile { get; }

        string? LoginName { get; }

        string? Email { get; }

        bool EmailConfirmed { get; }

        bool MobileConfirmed { get; }

        bool TwoFactorEnabled { get; }

        string? AccessToken { get; }

        string? RefreshToken { get; }

        //DateTimeOffset? TokenCreatedTime { get; }

        //long? ExpiredAt { get; }

        public bool IsLogined() => UserId.HasValue && AccessToken.IsNotNullOrEmpty();

        public void OnTokenFetched(ITokenRes tokenRes);

        public void OnTokenDeleted();

        public void OnTokenRefreshFailed();
    }
}