using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class BucketSettings
    {
        public string BucketName { get; set; }

        public string ReadArn { get; set; }

        public string WriteArn { get; set; }

        public string UserDirectoryPath { get; set; }

        public int StsExpireSeconds { get; set; } = 3600;
    }

    public class AliyunOssOptions : IOptions<AliyunOssOptions>
    {
        public AliyunOssOptions Value {
            get {
                return this;
            }
        }

        public string ProductName { get; set; }

        public IList<BucketSettings> Buckets { get; set; } = new List<BucketSettings>();

        public BucketSettings GetBucketSettings(string bucketName)
        {
            return Buckets.FirstOrDefault(s => s.BucketName.Equals(bucketName, GlobalSettings.Comparison));
        }
    }
}
