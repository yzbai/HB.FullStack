using HB.Component.Resource.Sms.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Resource.Sms
{
    public interface ISmsService
    {
        /// <summary>
        /// 用于验证用户手机
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<SendResult> SendValidationCode(string mobile, out string code);

        bool Validate(string mobile, string code);

    }
}
