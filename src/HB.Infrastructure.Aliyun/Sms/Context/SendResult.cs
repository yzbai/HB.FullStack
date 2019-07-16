using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class SendResult
    { 
        public bool IsSuccessful()
        {
            return "OK".Equals(Code, GlobalSettings.ComparisonIgnoreCase);
        }

        public string Message { get; set; }

        public string Code { get; set; }
    }
}
