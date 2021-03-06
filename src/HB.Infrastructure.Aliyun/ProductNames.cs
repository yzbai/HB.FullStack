using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    internal class AliyunProductNames
    {
        public const string SMS = "Dysmsapi";

        public const string OSS = "Oss";

        public const string STS = "Sts";
    }

    public enum AliyunProduct
    {
        Sms,
        Oss,
        Sts
    }
}
