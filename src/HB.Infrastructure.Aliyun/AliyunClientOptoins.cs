using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class AliyunClientOptoins : IOptions<AliyunClientOptoins>
    {
        public AliyunClientOptoins Value { get { return this; } }

        public IDictionary<string, AliyunServiceOptions> Configurations { get; set; }
    }

    public class AliyunServiceOptions : IOptions<AliyunServiceOptions>
    {
        public AliyunServiceOptions Value { get { return this; } }

        public string RegionId { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string Endpoint { get; set; }

    }
}
