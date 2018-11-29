using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Resource.Sms
{
    public interface ISmsCodeBiz
    {
        string GenerateNewSmsCode(int codeLength);
        void CacheSmsCode(string mobile, string cachedSmsCode, int expireMinutes);
        string GetSmsCodeFromCache(string mobile);
    }
}
