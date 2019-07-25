using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IList<EndpointSettings> Endpoints { get; private set; } = new List<EndpointSettings>();

        public void AddEndpoint(string productType, string version, string tokenRefreshResourceName, string url)
        {
            if (!Endpoints.Any(e => e.ProductType.Equals(productType, GlobalSettings.ComparisonIgnoreCase)
            && e.Version.Equals(version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(new EndpointSettings {
                    ProductType = productType,
                    Version = version,
                    TokenRefreshResourceName = tokenRefreshResourceName,
                    Url = url
                });
            }
        }
    }

    public class EndpointSettings
    {
        public string ProductType { get; set; }

        public string Version { get; set; }

        public string Url { get; set; }

        public string TokenRefreshProductType { get; set; }

        public string TokenRefreshVersion { get; set; }

        public string TokenRefreshResourceName { get; set; }

        public int TokenRefreshIntervalSeconds { get; set; } = 300;

        public bool NeedHttpMethodOveride { get; set; } = true;

        public static string GetHttpClientName(EndpointSettings endpoint)
        {
            ThrowIf.Null(endpoint, nameof(endpoint));

            return endpoint.ProductType + "_" + endpoint.Version;
        }
    }
}
