using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsOptions : IOptions<AliyunSmsOptions>
    {
        public string ProductName { get; set; }

        public AliyunSmsOptions Value { get { return this; } }

        public string SignName { get; set; }

        public TemplateIdentityValidation TemplateIdentityValidation { get; set; }

        //public string ParamSmsMobile { get; set; } = "Mobile";

        //public string ParamSmsMobileValue { get; set; } = "MobileCode";
    }

    public class TemplateIdentityValidation
    {
        public string TemplateCode { get; set; }

        public string ParamProduct { get; set; }

        public string ParamCode { get; set; }

        public int CodeLength { get; set; } = 6;

        public string ParamProductValue { get; set; }

        public int ExpireMinutes { get; set; } = 10;
    }
}
