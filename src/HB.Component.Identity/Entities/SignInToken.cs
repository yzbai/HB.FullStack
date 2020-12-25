using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Identity.Entities
{
    public class SignInToken : IdGenEntity
    {
        [Required]
        [ForeignKey(typeof(User))]
        [EntityProperty(NotNull = true)]
        public long UserId { get; set; }

        [Required]
        [EntityProperty(NotNull = true, NeedIndex = true)]
        public string RefreshToken { get; set; } = default!;

        [EntityProperty]
        public DateTimeOffset? ExpireAt { get; set; }

        [EntityProperty]
        public long RefreshCount { get; set; } = 0;

        [EntityProperty]
        public bool Blacked { get; set; } = false;


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