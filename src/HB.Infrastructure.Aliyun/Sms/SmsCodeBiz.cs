using HB.Framework.Common;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class SmsCodeBiz : ISmsCodeBiz
    {
        private IDistributedCache _cache;

        public SmsCodeBiz(IDistributedCache cache)
        {
            _cache = cache;
        }

        public void CacheSmsCode(string mobile, string cachedSmsCode, int expireMinutes)
        {
            _cache.SetString(
                        GetCachedKey(mobile),
                        cachedSmsCode,
                        new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expireMinutes)
                        });
        }

        public string GenerateNewSmsCode(int codeLength)
        {
            return SecurityHelper.CreateRandomNumbericString(codeLength);
        }

        private static string GetCachedKey(string mobile)
        {
            return mobile + "_vlc";
        }

        public string GetSmsCodeFromCache(string mobile)
        {
            return _cache.GetString(GetCachedKey(mobile));
        }
    }
}
