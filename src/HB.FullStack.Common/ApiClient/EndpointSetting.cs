using System;
using System.Collections.Generic;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// Endpoint(站点)的描述文档
    /// </summary>
    public class EndpointSetting
    {
        public string EndpointName { get; set; } = null!;

        public string? Version { get; set; }

        public Uri? BaseUrl { get; set; }

        public IList<ResBinding> ResBindings { get; set; } = new List<ResBinding>();

        #region Server系统设置

        public HttpMethodOverrideMode HttpMethodOverrideMode { get; set; }

        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = "Bearer";

        public Version HttpVersion { get; set; } = System.Net.HttpVersion.Version20;

        public int UserTokenRefreshIntervalSeconds { get; set; } = 300;

        public string UserAgent { get; set; } = "HB.FullStack.ApiClient";

        #endregion

        public override int GetHashCode()
        {
            return HashCode.Combine(EndpointName, Version, BaseUrl, HttpMethodOverrideMode, Challenge);
        }
    }

    public static class EndpointSettingExtensions
    {
        public static string GetHttpClientName(this EndpointSetting endpointSettings)
        {
            return $"{endpointSettings.EndpointName}_{endpointSettings.Version ?? "0"}";
        }
    }
}
