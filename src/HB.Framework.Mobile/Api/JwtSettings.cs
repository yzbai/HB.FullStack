using HB.Framework.Common;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Client.Api
{
    public class JwtSettings : ValidatableObject
    {
        /// <summary>
        /// 刷新token的站点名
        /// </summary>
        [Required]
        public string? ProductName { get; set; }

        /// <summary>
        /// 刷新token的站点资源名
        /// </summary>
        [Required]
        public string? ResourceName { get; set; }

        /// <summary>
        /// 刷新token的站点版本
        /// </summary>
        [Required]
        public string? Version { get; set; }

        

        public int RefreshIntervalSeconds { get; set; } = 300;

    }
}
