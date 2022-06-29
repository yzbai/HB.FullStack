using HB.FullStack.Database.Entities;

using System;

namespace HB.FullStack.Client.Maui.File
{
    [ClientEntity(
      expirySeconds: 3600,
      needLogined: true,
      allowOfflineRead: false,
      allowOfflineWrite: false)]
    public class AliyunStsToken : GuidEntity
    {
        public Guid UserId { get; set; }

        public string SecurityToken { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        public DateTimeOffset ExpirationAt { get; set; }

        public string DirectoryPermissionName { get; set; } = null!;

        public bool ReadOnly { get; set; }
    }

    public static class AliyunStsTokenExtensions
    {
        private static TimeSpan _gapTime = TimeSpan.FromMinutes(1);

        public static bool IsExpired(this AliyunStsToken token)
        {
            return token.ExpirationAt - TimeUtil.UtcNow < _gapTime;
        }
    }
}