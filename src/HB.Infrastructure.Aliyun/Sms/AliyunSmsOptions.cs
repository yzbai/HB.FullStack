using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsOptions : IOptions<AliyunSmsOptions>
    {
        public AliyunSmsOptions Value { get { return this; } }

        public string RegionId { get; set; }

        public string Endpoint { get; set; }

        public string AccessUserName { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string SignName { get; set; }

        public TemplateIdentityValidation TemplateIdentityValidation { get; set; }

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
