using System;
using System.Collections.Generic;
using System.Net.Http;

using HB.FullStack.Common.Api;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value => this;

        public IList<SiteSetting> SiteSettings { get; set; } = new List<SiteSetting>();

        public IList<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public Func<HttpMessageHandler>? ConfigureHttpMessageHandler { get; set; }

    }

}
