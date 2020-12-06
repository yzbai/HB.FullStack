using System.Threading.Tasks;

namespace HB.FullStack.Common.Server
{
    public interface ISmsService
    {
        void SendValidationCode(string mobile/*, out string code*/);
        Task<bool> ValidateAsync(string mobile, string code);

#if DEBUG
        void SendValidationCode(string mobile, string smsCode, int expiryMinutes);
#endif

    }
}