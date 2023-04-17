using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Server.Identity.Models
{
    public class SignInCredential : TimestampGuidDbModel
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [Required]
        [DbField(NotNull = true, NeedIndex = true)]
        public string RefreshToken { get; set; } = default!;

        public DateTimeOffset? ExpireAt { get; set; }

        public long RefreshCount { get; set; }

        public bool Blacked { get; set; }

        #region Client

        [Required]
        [DbField(NotNull = true, NeedIndex = true)]
        public string ClientId { get; set; } = default!;

        [DbField(NotNull = true)]
        public string ClientVersion { get; set; } = default!;

        [DbField(NotNull = true)]
        public string ClientIp { get; set; } = default!;

        #endregion

        #region Device Infos

        [Required]
        public string DeviceName { get; set; } = null!;

        [Required]
        public string DeviceModel { get; set; } = null!;

        [Required]
        public string DeviceOSVersion { get; set; } = null!;

        [Required]
        public string DevicePlatform { get; set; } = null!;

        [Required]
        public DeviceIdiom DeviceIdiom { get; set; } = DeviceIdiom.Unknown;

        [Required]
        public string DeviceType { get; set; } = null!;

        #endregion

        public SignInCredential()
        {
        }

        public SignInCredential(
            Guid userId,
            string refreshToken,
            DateTimeOffset? expireAt,
            string clientId,
            string clientVersion,
            string clientIp,
            string deviceName,
            string deviceModel,
            string deviceOSVersion,
            string devicePlatform,
            DeviceIdiom deviceIdiom,
            string deviceType)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            ExpireAt = expireAt;

            ClientId = clientId;
            ClientVersion = clientVersion;
            ClientIp = clientIp;

            DeviceName = deviceName;
            DeviceModel = deviceModel;
            DeviceOSVersion = deviceOSVersion;
            DevicePlatform = devicePlatform;
            DeviceIdiom = deviceIdiom;
            DeviceType = deviceType;
        }
    }
}