using HB.FullStack.Cache;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HB.FullStack.Server
{
    internal class PublicResourceTokenManager : IPublicResourceTokenManager
    {
        private const string _prefix = "PRT_";
        private readonly int _tokenlength = 36 + _prefix.Length;
        private const string _defaultValue = "0";
        private readonly ICache _cache;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger _logger;

        public PublicResourceTokenManager(ICache cache, IDataProtectionProvider dataProtectionProvider, ILogger<PublicResourceTokenManager> logger)
        {
            _cache = cache;
            _logger = logger;

            _dataProtector = dataProtectionProvider.CreateProtector(nameof(PublicResourceTokenManager));
        }

        /// <exception cref="EncoderFallbackException"></exception>
        public async Task<string> GetNewTokenAsync(int expiredSeconds)
        {
            string token = _prefix + Guid.NewGuid().ToString();

            await _cache.SetString2Async(token, _defaultValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiredSeconds) }).ConfigureAwait(false);

            return _dataProtector.Protect(token);
        }

        public async Task<bool> CheckTokenAsync(string? protectedToken)
        {
            if (protectedToken.IsNullOrEmpty())
            {
                return false;
            }

            string token;

            try
            {
                token = _dataProtector.Unprotect(protectedToken);

                if (token.IsNullOrEmpty() || token.Length != _tokenlength || !token.StartsWith(_prefix, GlobalSettings.Comparison))
                {
                    _logger.LogWarning($"UnProtected Failed. May has an attack. Input:{protectedToken}.");
                    return false;
                }
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, $"protectedToken:{protectedToken}");
                return false;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, $"protectedToken:{protectedToken}");
                return false;
            }


            return await _cache.IsExistThenRemoveAsync(token).ConfigureAwait(false);
        }


    }
}
