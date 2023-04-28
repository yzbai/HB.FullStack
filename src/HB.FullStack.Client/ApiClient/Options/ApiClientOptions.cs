using System;
using System.Collections.Generic;
using System.Net.Http;

using HB.FullStack.Common.Shared;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value => this;

        //TODO: 目前只能向一个站点进行登录，以后考虑多站点登录
        public SiteSetting TokenSiteSetting { get; set; } = null!;

        public IList<SiteSetting> OtherSiteSettings { get; set; } = new List<SiteSetting>();

        public IList<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

        public int TokenRefreshIntervalSeconds { get; set; } = 300;

        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public Func<HttpMessageHandler>? ConfigureHttpMessageHandler { get; set; }

        public string UserAgent { get; set; } = "HB.FullStack.ApiClient";
    }

}
