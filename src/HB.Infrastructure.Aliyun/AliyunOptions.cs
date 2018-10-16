using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class AliyunOptions : IOptions<AliyunOptions>
    {
        public AliyunOptions Value => this;

        public IList<AliyunProductOptions> Products { get; set; }
    }

    public class AliyunProductOptions : IOptions<AliyunProductOptions>
    {
        public AliyunProductOptions Value => this;

        public string ProductName { get; set; }

        public string RegionId { get; set; }

        public string AccessUserName { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string Endpoint { get; set; }
    }
}
