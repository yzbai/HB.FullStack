using HB.FullStack.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using HB.Component.Identity.Entities;
using HB.FullStack.Common.Entities;

namespace HB.Component.Authorization.Entities
{
    [DatabaseEntity]
    public class SignInToken : Entity
    {
        [Required]
        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = default!;

        [Required]
        [EntityProperty(NotNull = true)]
        public string RefreshToken { get; set; } = default!;

        [EntityProperty]
        public DateTimeOffset? ExpireAt { get; set; }

        [EntityProperty]
        public long RefreshCount { get; set; } = 0;

        [EntityProperty]
        public bool Blacked { get; set; } = false;


        #region Device

        [Required]
        [EntityProperty(NotNull = true)]
        public string DeviceId { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        public string DeviceVersion { get; set; } = default!;


        [EntityProperty(NotNull = true)]
        public string DeviceIp { get; set; } = default!;

        #endregion

        #region Device Infos

        [Required]
        [EntityProperty]
        public string DeviceName { get; set; } = null!;

        [Required]
        [EntityProperty]
        public string DeviceModel { get; set; } = null!;

        [Required]
        [EntityProperty]
        public string DeviceOSVersion { get; set; } = null!;

        [Required]
        [EntityProperty]
        public string DevicePlatform { get; set; } = null!;

        [Required]
        [EntityProperty]
        public DeviceIdiom DeviceIdiom { get; set; } = DeviceIdiom.Unknown;

        [Required]
        [EntityProperty]
        public string DeviceType { get; set; } = null!;

        #endregion
    }

}