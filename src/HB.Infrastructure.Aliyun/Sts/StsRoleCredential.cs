using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class StsRoleCredential
    {
        public string RequestId { get; set; }

        public string SecurityToken { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string Expiration { get; set; }

        public string AssumedRoleId { get; set; }

        public string AssumedRoleName { get; set; }

    }
}
