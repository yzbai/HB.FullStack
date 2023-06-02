using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    [DbModel(ConflictCheckMethods = ConflictCheckMethods.Timestamp)]
    public class TokenCredential<TId> : DbModel<TId>, ITimestamp
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User<>), false)]
        public TId UserId { get; set; } = default!;

        [Required]
        [DbField(NotNull = true, NeedIndex = true)]
        public string RefreshToken { get; set; } = default!;

        public DateTimeOffset? RefreshTokenExpireAt { get; set; }

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

        public TokenCredential()
        { }

        public TokenCredential(
            TId userId,
            string refreshToken,
            DateTimeOffset? refreshTokenExpiredAt,
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
            RefreshTokenExpireAt = refreshTokenExpiredAt;

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

        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}