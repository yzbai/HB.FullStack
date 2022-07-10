using HB.FullStack.Cache;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HB.FullStack.WebApi
{
    public class CommonResourceTokenService : ICommonResourceTokenService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ILogger _logger;

        public CommonResourceTokenService(IModelCache cache, IDataProtectionProvider dataProtectionProvider, ILogger<CommonResourceTokenService> logger)
        {
            _logger = logger;

            _dataProtector = dataProtectionProvider.CreateProtector(nameof(CommonResourceTokenService));
        }

        public bool TryCheckToken(string? protectedToken, [NotNullWhen(true)] out string? content)
        {
            content = null;

            if (protectedToken.IsNullOrEmpty())
            {
                return false;
            }


            try
            {
                content = _dataProtector.Unprotect(protectedToken);

                if (content.IsNullOrEmpty())
                {
                    _logger.LogWarning("UnProtected Failed. May has an attack. {protectedToken}.", content);
                    return false;
                }

                return true;
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "{protectedToken}", protectedToken);
                return false;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "{protectedToken}", protectedToken);
                return false;
            }
        }

        public string BuildNewToken(string content)
        {
            return _dataProtector.Protect(content);
        }
    }
}