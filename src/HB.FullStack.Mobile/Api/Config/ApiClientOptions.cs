using HB.FullStack.Common.Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.FullStack.Client.Api
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        private IDictionary<string, string>? _apiKeysDict;

        public ApiClientOptions Value => this;

        public IList<EndpointSettings> Endpoints { get; private set; } = new List<EndpointSettings>();

        public IList<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

        public void AddEndpoint(EndpointSettings endpointSettings)
        {
            if (!Endpoints.Any(e => e.Name!.Equals(endpointSettings.Name, GlobalSettings.ComparisonIgnoreCase)
            && e.Version!.Equals(endpointSettings.Version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(endpointSettings);
            }
        }

        public bool TryGetApiKey(string? name, out string key)
        {
            if (string.IsNullOrEmpty(name))
            {
                key = string.Empty;
                return false;
            }

            if (_apiKeysDict == null)
            {
                _apiKeysDict = new Dictionary<string, string>();

                ApiKeys.ForEach(apiKey => _apiKeysDict.Add(apiKey.Name, apiKey.Key));
            }

            return _apiKeysDict.TryGetValue(name, out key);
        }
    }

}
