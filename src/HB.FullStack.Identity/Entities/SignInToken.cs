﻿using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.Identity.Entities
{
    public class SignInToken : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User))]
        public Guid UserId { get; set; }

        [Required]
        [EntityProperty(NotNull = true, NeedIndex = true)]
        public string RefreshToken { get; set; } = default!;

        
        public DateTimeOffset? ExpireAt { get; set; }

        
        public long RefreshCount { get; set; }

        
        public bool Blacked { get; set; }


        #region Device

        [Required]
        [EntityProperty(NotNull = true, NeedIndex = true)]
        public string DeviceId { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        public string DeviceVersion { get; set; } = default!;


        [EntityProperty(NotNull = true)]
        public string DeviceIp { get; set; } = default!;

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

        public SignInToken() { }

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