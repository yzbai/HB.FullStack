using System;
using System.Collections.Generic;

namespace HB.FullStack.Client.ApiClient
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

        #region Site SystemSettings
        public bool UseHttpMethodOverride { get; set; } = true;

        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge { get; set; } = "Bearer";

        public Version HttpVersion { get; set; } = System.Net.HttpVersion.Version20;

        #endregion

        public override int GetHashCode()
        {
            return HashCode.Combine(SiteName, Version, BaseUrl, UseHttpMethodOverride, Challenge, HttpVersion);
        }
    }

    public static class SiteSettingExtensions
    {
        public static string GetHttpClientName(this SiteSetting siteSettings)
        {
            return $"{siteSettings.SiteName}_{siteSettings.Version ?? "0"}";
        }
    }
}
