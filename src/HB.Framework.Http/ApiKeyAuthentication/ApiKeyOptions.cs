using HB.Framework.Common.Api;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System;

namespace HB.Framework.Http.ApiKeyAuthentication
{
    public class ApiKeyOptions : AuthenticationSchemeOptions
    {
        private IDictionary<string, string>? _apiKeysDict;

        public const string DefaultScheme = "ApiKey";

        public string Scheme { get; set; } = DefaultScheme;

        public IList<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

        public bool TryGetApiKey(string key, out string? name)
        {
            if (_apiKeysDict == null)
            {
                _apiKeysDict = new Dictionary<string, string>();

                ApiKeys.ForEach(apiKey => _apiKeysDict.Add(apiKey.Key, apiKey.Name));
            }

            return _apiKeysDict.TryGetValue(key, out name);
        }
    }
}
