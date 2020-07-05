using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HB.Framework.Http
{
    internal class PublicResourceTokenManager : IPublicResourceTokenManager
    {
        private const string _prefix = "PRT_";
        private readonly int _tokenlength = 36 + _prefix.Length;
        private const string _defaultValue = "0";
        private readonly IDistributedCache _cache;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger _logger;

        public PublicResourceTokenManager(IDistributedCache cache, IDataProtectionProvider dataProtectionProvider, ILogger<PublicResourceTokenManager> logger)
        {
            _cache = cache;
            _logger = logger;

            _dataProtector = dataProtectionProvider.CreateProtector(nameof(PublicResourceTokenManager));
        }

        /// <exception cref="EncoderFallbackException"></exception>
        public async Task<string> GetNewToken(int expiredSeconds)
        {
            string token = _prefix + Guid.NewGuid().ToString();

            await _cache.SetStringAsync(token, _defaultValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiredSeconds) }).ConfigureAwait(false);

            return _dataProtector.Protect(token);
        }

        public async Task<bool> CheckToken(string protectedToken)
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
