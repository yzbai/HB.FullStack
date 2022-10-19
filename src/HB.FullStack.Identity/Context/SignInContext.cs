using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;

namespace HB.FullStack.Identity
{
    public class SignInContext : ValidatableObject
    {
        //public HttpContext HttpContext { get; set; }

        public SignInType SignInType { get; set; }

        [LoginName]
        public string? LoginName { get; set; }

        [Password]
        public string? Password { get; set; }

        [Mobile]
        public string? Mobile { get; set; }

        public bool RememberMe { get; set; }

        [Required]
        public string DeviceId { get; set; } = default!;

        public DeviceInfos DeviceInfos { get; set; } = default!;

        [Required]
        public string DeviceVersion { get; set; } = default!;

        [Required]
        public string DeviceIp { get; set; } = default!;

        /// <summary>
        /// Audience
        /// </summary>
        [Required]
        public string SignToWhere { get; set; } = default!;

        public LogOffType LogOffType { get; set; } = LogOffType.None;
    }
}