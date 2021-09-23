using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Identity.ModelObjects
{
    public class SignInToken : ModelObject
    {
        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [Required]
        public string RefreshToken { get; set; } = default!;

        
        public DateTimeOffset? ExpireAt { get; set; }

        
        public long RefreshCount { get; set; }

        
        public bool Blacked { get; set; }


        #region Device

        [Required]
        public string DeviceId { get; set; } = default!;

        public string DeviceVersion { get; set; } = default!;

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
    }

}