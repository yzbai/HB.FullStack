using HB.Framework.Common;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Client.Api
{
    public class TokenRefreshSettings : ValidatableObject
    {
        /// <summary>
        /// 刷新token的站点名
        /// </summary>
        [Required]
        public string? TokenRefreshProductType { get; set; }

        /// <summary>
        /// 刷新token的站点版本
        /// </summary>
        [Required]
        public string? TokenRefreshVersion { get; set; }

        /// <summary>
        /// 刷新token的站点资源名
        /// </summary>
        [Required]
        public string? TokenRefreshResourceName { get; set; }

        public int TokenRefreshIntervalSeconds { get; set; } = 300;

    }
}
