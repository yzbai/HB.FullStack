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

}
