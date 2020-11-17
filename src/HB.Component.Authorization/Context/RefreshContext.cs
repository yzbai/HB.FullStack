using HB.Framework.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Authorization.Abstractions
{
    public class RefreshContext : ValidatableObject
    {
        [Required]
        public string AccessToken { get; set; } = default!;

        [Required]
        public string RefreshToken { get; set; } = default!;

        [Required]
        public string DeviceId { get; set; } = default!;
        public DeviceInfos DeviceInfos { get; set; } = default!;
        public string DeviceVersion { get; set; } = default!;
        //public string DeviceAddress { get; set; } = default!;
    }
}
