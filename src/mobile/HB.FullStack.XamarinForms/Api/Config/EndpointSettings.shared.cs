using HB.FullStack.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.XamarinForms.Api
{
    public class EndpointSettings : ValidatableObject
    {
        /// <summary>
        /// 产品名，一般为站点类名
        /// </summary>
        [Required]
        public string? Name { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        //[Required]
        public string? Version { get; set; }

        /// <summary>
        /// url地址
        /// </summary>
        [Required]
        public Uri? Url { get; set; }

        public bool NeedHttpMethodOveride { get; set; } = true;

        public JwtEndpointSetting JwtEndpoint { get; set; } = new JwtEndpointSetting();


        [JsonIgnore]
        public string HttpClientName => $"{Name}_{Version}";
    }
}
