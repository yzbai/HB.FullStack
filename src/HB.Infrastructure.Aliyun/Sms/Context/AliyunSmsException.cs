using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsException : Exception
    {
        public AliyunSmsException(string code, string message) : base(message)
        {
            Code = code;
        }


        //public string Message { get; set; }

        public string Code { get; set; }
    }
}
