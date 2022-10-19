using System;
using System.Collections.Generic;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    /// <summary>
    /// Endpoint(站点)的描述文档
    /// </summary>
    public class SiteSetting
    {
        public string SiteName { get; set; } = null!;

        public string? Version { get; set; }

        public Uri? BaseUrl { get; set; }

        public IList<ResEndpoint> Endpoints { get; set; } = new List<ResEndpoint>();

        #region 由 Site 决定的设置

        public bool UseHttpMethodOverride { get; set; } = true;

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
            return HashCode.Combine(SiteName, Version, BaseUrl, UseHttpMethodOverride, Challenge);
        }
    }
}
