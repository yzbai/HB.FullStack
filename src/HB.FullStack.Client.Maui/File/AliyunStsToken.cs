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

        public string DirectoryRegExp { get; set; } = null!;

        public bool ReadOnly { get; set; }
    }
}