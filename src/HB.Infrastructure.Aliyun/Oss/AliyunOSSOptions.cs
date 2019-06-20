using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class BucketSettings
    {
        public string RegionId { get; set; }

        public string Endpoint { get; set; }

        public string AccessUserName { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string BucketName { get; set; }

        public string BucketUserDirectory { get; set; }

        public OssStsSettings Sts { get; set; }
    }

    public class OssStsSettings
    {
        public string Endpoint { get; set; }

        public int ExpireSeconds { get; set; } = 3600;

        public string ReadArn { get; set; }

        public string WriteArn { get; set; }
    }

    public class AliyunOssOptions : IOptions<AliyunOssOptions>
    {
        public AliyunOssOptions Value => this;

        /// <summary>
        /// 用于存储用户数据的Bucket
        /// </summary>
        public string UserBucketName { get; set; }

        /// <summary>
        /// 用于公共数据的Bucket
        /// </summary>
        public string PublicBucketName { get; set; }

        public IList<BucketSettings> Buckets { get; set; } = new List<BucketSettings>();
    }
}
