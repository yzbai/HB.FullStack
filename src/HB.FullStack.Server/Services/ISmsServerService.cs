using System;
using System.Threading.Tasks;

namespace HB.FullStack.Server.Services
{
    /// <summary>
    /// 用于Server端调用第三方服务
    /// </summary>
    public interface ISmsServerService
    {
        Task SendValidationCodeAsync(string mobile/*, out string code*/);


        Task<bool> ValidateAsync(string mobile, string code);

#if DEBUG
        Task SendValidationCodeAsync(string mobile, string smsCode, int expiryMinutes);
#endif

    }
}