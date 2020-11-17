using HB.Framework.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using HB.Component.Identity.Entities;
using HB.Framework.Common.Entities;

namespace HB.Component.Authorization.Entities
{
    [KVStoreEntity]
    public class SignInToken : Entity
    {
        [Required]
        [ForeignKey(typeof(IdentityUser))]
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

        [Required]
        [EntityProperty(NotNull = true, Converter = typeof(DeviceInfosDatabaseTypeConverter))]
        public DeviceInfos DeviceInfos { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        public string DeviceVersion { get; set; } = default!;

        [EntityProperty(NotNull = false)]
        public string? DeviceAddress { get; set; }

        [EntityProperty(NotNull = true)]
        public string DeviceIp { get; set; } = default!;

        #endregion
    }

    public class DeviceInfosDatabaseTypeConverter : DatabaseTypeConverter
    {
        protected override object? StringDbValueToTypeValue(string stringValue)
        {
            return SerializeUtil.FromJson<DeviceInfos>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }
}