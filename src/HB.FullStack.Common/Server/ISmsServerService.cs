using System;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Server
{
    public interface ISmsServerService
    {
        /// <exception cref="SmsException"></exception>
        void SendValidationCode(string mobile/*, out string code*/);


        /// <exception cref="SmsException"></exception>
        Task<bool> ValidateAsync(string mobile, string code);

#if DEBUG
        /// <exception cref="SmsException"></exception>
        void SendValidationCode(string mobile, string smsCode, int expiryMinutes);
#endif

    }
}