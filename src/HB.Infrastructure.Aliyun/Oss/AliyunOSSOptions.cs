using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssOptions : IOptions<AliyunOssOptions>
    {
        public AliyunOssOptions Value => this;

        public string ProductName { get; set; }
    }
}
