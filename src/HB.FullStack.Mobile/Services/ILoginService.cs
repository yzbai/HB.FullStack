using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services
{
    public interface ILoginService
    {
        Task<bool> LoginBySmsCodeAsync(string mobile, string smsCode);

        Task RequestSmsCodeAsync(string mobile, Func<Task>? onSuccessDelegate);
    }
}
