using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class AliyunOptions : IOptions<AliyunOptions>
    {
        public AliyunOptions Value => this;

        public IList<AliyunAccessOptions> Accesses { get; set; }
    }

    public class AliyunAccessOptions : IOptions<AliyunAccessOptions>
    {
        public AliyunAccessOptions Value => this;

        public string ProductName { get; set; }

        public string RegionId { get; set; }

        public string AccessUserName { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string Endpoint { get; set; }
    }
}
