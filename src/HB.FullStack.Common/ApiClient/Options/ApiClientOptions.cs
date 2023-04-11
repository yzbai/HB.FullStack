using System;
using System.Collections.Generic;
using System.Net.Http;

using HB.FullStack.Common.Shared;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value => this;

        public SiteSetting SignInReceiptSiteSetting { get; set; } = null!;

        public IList<SiteSetting> OtherSiteSettings { get; set; } = new List<SiteSetting>();

        public IList<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

        public int SignInReceiptRefreshIntervalSeconds { get; set; } = 300;

        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public Func<HttpMessageHandler>? ConfigureHttpMessageHandler { get; set; }

        public string UserAgent { get; set; } = "HB.FullStack.ApiClient";
    }

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
