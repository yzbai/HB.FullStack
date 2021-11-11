using HB.FullStack.Database.Entities;

using MessagePack;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    [MessagePackObject]
    public class SignInToken : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        [MessagePack.Key(7)]
        public Guid UserId { get; set; }

        [Required]
        [EntityProperty(NotNull = true, NeedIndex = true)]
        [MessagePack.Key(8)]
        public string RefreshToken { get; set; } = default!;

        [MessagePack.Key(9)]
        public DateTimeOffset? ExpireAt { get; set; }

        [MessagePack.Key(10)]
        public long RefreshCount { get; set; }

        [MessagePack.Key(11)]
        public bool Blacked { get; set; }

        #region Device

        [Required]
        [EntityProperty(NotNull = true, NeedIndex = true)]
        [MessagePack.Key(12)]
        public string DeviceId { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        [MessagePack.Key(13)]
        public string DeviceVersion { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        [MessagePack.Key(14)]
        public string DeviceIp { get; set; } = default!;

        #endregion

        #region Device Infos

        [Required]
        [MessagePack.Key(15)]
        public string DeviceName { get; set; } = null!;

        [Required]
        [MessagePack.Key(16)]
        public string DeviceModel { get; set; } = null!;

        [Required]
        [MessagePack.Key(17)]
        public string DeviceOSVersion { get; set; } = null!;

        [Required]
        [MessagePack.Key(18)]
        public string DevicePlatform { get; set; } = null!;

        [Required]
        [MessagePack.Key(19)]
        public DeviceIdiom DeviceIdiom { get; set; } = DeviceIdiom.Unknown;

        [Required]
        [MessagePack.Key(20)]
        public string DeviceType { get; set; } = null!;

        #endregion

        public SignInToken()
        {
        }

        public SignInToken(
            Guid userId,
            string refreshToken,
            DateTimeOffset? expireAt,
            string deviceId,
            string deviceVersion,
            string deviceIp,
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

            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceIp = deviceIp;

            DeviceName = deviceName;
            DeviceModel = deviceModel;
            DeviceOSVersion = deviceOSVersion;
            DevicePlatform = devicePlatform;
            DeviceIdiom = deviceIdiom;
            DeviceType = deviceType;
        }
    }
}