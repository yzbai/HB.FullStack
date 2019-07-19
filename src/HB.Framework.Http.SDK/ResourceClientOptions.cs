using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HB.Framework.Http.SDK
{
    public class ResourceClientOptions : IOptions<ResourceClientOptions>
    {
        public ResourceClientOptions Value {
            get {
                return this;
            }
        }

        public int TokenRefreshIntervalSeconds { get; set; } = 300;

        public IDictionary<string, Uri> Endpoints { get; private set; } = new Dictionary<string, Uri>();

        public TokenRefreshSettings TokenRefreshSettings { get; set; } = new TokenRefreshSettings();

        public void AddEndpoint(string productType, Uri url)
        {
            Endpoints.Add(productType, url);
        }
    }

    public class TokenRefreshSettings
    {
        public string ProductType { get; set; }

        public string Version { get; set; }

        public string ResourceName { get; set; }
    }
}
