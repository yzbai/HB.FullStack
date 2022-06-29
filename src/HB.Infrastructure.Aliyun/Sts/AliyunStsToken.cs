using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class AliyunStsToken
    {
        public string RequestId { get; set; } = null!;

        public string SecurityToken { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        public DateTimeOffset ExpirationAt { get; set; }

        public string ArId { get; set; } = null!;

        public string Arn { get; set; } = null!;

        public string DirectoryPermissionName { get; set; } = null!;

        public bool ReadOnly { get; set; }

    }
}
