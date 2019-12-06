using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.Client.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value {
            get {
                return this;
            }
        }

        public IList<EndpointSettings> Endpoints { get; private set; } = new List<EndpointSettings>();

        public void AddEndpoint(EndpointSettings endpointSettings)
        {
            if (!Endpoints.Any(e => e.ProductType.Equals(endpointSettings.ProductType, GlobalSettings.ComparisonIgnoreCase)
            && e.Version.Equals(endpointSettings.Version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(endpointSettings);
            }
        }
    }

    public class TokenRefreshSettings
    {
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

        public bool NeedHttpMethodOveride { get; set; } = true;

        public TokenRefreshSettings TokenRefresh = new TokenRefreshSettings();

        public static string GetHttpClientName(EndpointSettings endpoint)
        {
            ThrowIf.Null(endpoint, nameof(endpoint));

            return endpoint.ProductType + "_" + endpoint.Version;
        }
    }
}
