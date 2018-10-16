using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun.Vod
{
    public class AliyunVodOptions : IOptions<AliyunVodOptions>
    {
        public AliyunVodOptions Value => this;

        public string ProductName { get; set; }
    }
}
