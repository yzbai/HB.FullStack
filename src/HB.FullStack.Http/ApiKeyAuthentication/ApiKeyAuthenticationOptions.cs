﻿using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Server.WebLib.ApiKeyAuthentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        private IDictionary<string, string>? _apiKeysDict;

        public const string DefaultScheme = "ApiKey";

        public string Scheme { get; set; } = DefaultScheme;

        public IList<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

        public bool TryGetApiKey(string key, [NotNullWhen(true)] out string? name)
        {
            if (_apiKeysDict == null)
            {
                _apiKeysDict = new Dictionary<string, string>();

                foreach (var apiKey in ApiKeys)
                {
                    _apiKeysDict.Add(apiKey.Key, apiKey.Name);
                }
            }

            return _apiKeysDict.TryGetValue(key, out name);
        }
    }
}
