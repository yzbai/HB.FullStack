using HB.FullStack.Common.Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiClientOptions : IOptions<ApiClientOptions>
    {
        public const string NO_BASEURL_HTTPCLIENT_NAME = nameof(NO_BASEURL_HTTPCLIENT_NAME);

        private IDictionary<string, string>? _apiKeysDict;

        public ApiClientOptions Value => this;

        public JwtEndpointSetting DefaultJwtEndpoint { get; set; } = null!;

        public IList<EndpointSettings> Endpoints { get; set; } = new List<EndpointSettings>();

        public IList<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();

        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(20);

        //public AsyncEventHandler<ApiRequest, ApiEventArgs>? OnRequestingAsync { get; set; }

        public void AddEndpoint(EndpointSettings endpointSettings)
        {
            if (!Endpoints.Any(e => e.Name!.Equals(endpointSettings.Name, GlobalSettings.ComparisonIgnoreCase)
            && e.Version!.Equals(endpointSettings.Version, GlobalSettings.ComparisonIgnoreCase)))
            {
                Endpoints.Add(endpointSettings);
            }
        }

        public bool TryGetApiKey(string? name, [NotNullWhen(true)] out string? key)
        {
            if (string.IsNullOrEmpty(name))
            {
                key = string.Empty;
                return false;
            }

            if (_apiKeysDict == null)
            {
                _apiKeysDict = new Dictionary<string, string>();

                foreach (var apiKey in ApiKeys)
                {
                    _apiKeysDict.Add(apiKey.Name, apiKey.Key);
                }
            }

            return _apiKeysDict.TryGetValue(name!, out key);
        }
    }

}
