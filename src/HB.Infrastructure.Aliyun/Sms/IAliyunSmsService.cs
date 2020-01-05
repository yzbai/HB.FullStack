using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Infrastructure.Aliyun.Sms
{
    public interface IAliyunSmsService
    {
        /// <summary>
        /// 用于验证用户手机
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        void SendValidationCode(string mobile/*, out string code*/);

        bool Validate(string mobile, string code);

    }
}
