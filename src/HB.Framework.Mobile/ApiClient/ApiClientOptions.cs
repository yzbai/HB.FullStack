using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.Mobile.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value {
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
        /// <summary>
        /// 产品名，一般为站点类名
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// url地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 刷新token的站点名
        /// </summary>
        public string TokenRefreshProductType { get; set; }

        /// <summary>
        /// 刷新token的站点版本
        /// </summary>
        public string TokenRefreshVersion { get; set; }

        /// <summary>
        /// 刷新token的站点资源名
        /// </summary>
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
