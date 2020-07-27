using HB.Framework.Common.Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.Client.Api
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        private IDictionary<string, string>? _apiKeysDict;

        public ApiClientOptions Value => this;

        public IList<EndpointSettings> Endpoints { get; private set; } = new List<EndpointSettings>();

        public IList<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

        public void AddEndpoint(EndpointSettings endpointSettings)
        {
            if (!Endpoints.Any(e => e.ProductName!.Equals(endpointSettings.ProductName, GlobalSettings.ComparisonIgnoreCase)
            && e.Version!.Equals(endpointSettings.Version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(endpointSettings);
            }
        }

        public bool TryGetApiKey(string name, out string key)
        {
            if (_apiKeysDict == null)
            {
                _apiKeysDict = new Dictionary<string, string>();

                ApiKeys.ForEach(apiKey => _apiKeysDict.Add(apiKey.Name, apiKey.Key));
            }

            return _apiKeysDict.TryGetValue(name, out key);
        }
    }

}
