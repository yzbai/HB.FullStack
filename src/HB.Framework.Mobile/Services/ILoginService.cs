using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Client.Services
{
    public interface ILoginService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">来源，比如PageName，或者Page.ButtonName</param>
        void PerformLogin();
        Task<bool> LoginBySmsCodeAsync(string mobile, string smsCode);
        Task RequestSmsCodeAsync(string mobile, Func<Task>? onSuccessDelegate);
    }
}
