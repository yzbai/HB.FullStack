using HB.FullStack.Database.DatabaseModels;

using System;

namespace HB.FullStack.Client.Maui.File
{
    [ClientModel(
      expirySeconds: int.MaxValue, //由StsToken.ExpirationAt业务逻辑决定
      needLogined: true,
      allowOfflineRead: false,
      allowOfflineWrite: false)]
    public class StsToken : GuidModel
    {
        public Guid UserId { get; set; }

        public string SecurityToken { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        public DateTimeOffset ExpirationAt { get; set; }

        public string DirectoryPermissionName { get; set; } = null!;

        public bool ReadOnly { get; set; }
    }

    public static class StsTokenExtensions
    {
        private static TimeSpan _gapTime = TimeSpan.FromMinutes(1);

        public static bool IsExpired(this StsToken token)
        {
            return token.ExpirationAt - TimeUtil.UtcNow < _gapTime;
        }
    }
}