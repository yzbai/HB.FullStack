using HB.FullStack.Common;
using HB.FullStack.Common.Api.Requests;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.ApiClient
{
    public class EndpointSettings : ValidatableObject
    {
        /// <summary>
        /// 产品名，一般为站点类名
        /// </summary>
        [Required]
        public string EndpointName { get; set; } = null!;

        /// <summary>
        /// 版本
        /// </summary>
        //[Required]
        public string? Version { get; set; }

        /// <summary>
        /// url地址
        /// </summary>
        [Required]
        public Uri? BaseUrl { get; set; }

        public HttpMethodOverrideMode HttpMethodOverrideMode { get; set; }

        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = "Bearer";

        public JwtEndpointSetting? JwtEndpoint { get; set; }


        [JsonIgnore]
        public string HttpClientName => $"{EndpointName}_{Version}";
    }
}
