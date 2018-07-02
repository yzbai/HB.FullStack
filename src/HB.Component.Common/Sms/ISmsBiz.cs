using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Compnent.Common.Sms
{
    public interface ISmsBiz
    {
        /// <summary>
        /// 用于验证用户手机
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<SmsResponseResult> SendIdentityValidationCode(string mobile, out string code);

    }
}
