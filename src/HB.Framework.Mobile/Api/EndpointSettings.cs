using HB.Framework.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Client.Api
{
    public class EndpointSettings : ValidatableObject
    {
        /// <summary>
        /// 产品名，一般为站点类名
        /// </summary>
        [Required]
        public string? ProductType { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        [Required]
        public string? Version { get; set; }

        /// <summary>
        /// url地址
        /// </summary>
        [Required]
        public Uri? Url { get; set; }

        public bool NeedHttpMethodOveride { get; set; } = true;

        public TokenRefreshSettings TokenRefreshSettings { get; set; } = new TokenRefreshSettings();


        public static string GetHttpClientName(EndpointSettings endpoint)
        {
            ThrowIf.Null(endpoint, nameof(endpoint));

            return endpoint.ProductType + "_" + endpoint.Version;
        }
    }
}
