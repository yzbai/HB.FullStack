using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.Client.Api
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public ApiClientOptions Value => this;

        public IList<EndpointSettings> Endpoints { get; private set; } = new List<EndpointSettings>();

        public void AddEndpoint(EndpointSettings endpointSettings)
        {
            if (!Endpoints.Any(e => e.ProductType!.Equals(endpointSettings.ProductType, GlobalSettings.ComparisonIgnoreCase)
            && e.Version!.Equals(endpointSettings.Version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(endpointSettings);
            }
        }
    }
}
