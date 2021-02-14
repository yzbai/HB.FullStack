using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class BucketSettings
    {
        [DisallowNull, NotNull]
        public string RegionId { get; set; } = null!;

        [DisallowNull, NotNull]
        public string Endpoint { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessLoginName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeyId { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeySecret { get; set; } = null!;

        [DisallowNull, NotNull]
        public string BucketName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string BucketUserDirectory { get; set; } = null!;

        public OssStsSettings Sts { get; set; } = new OssStsSettings();
    }

    public class OssStsSettings
    {
        [DisallowNull, NotNull]
        public string Endpoint { get; set; } = null!;

        public int ExpireSeconds { get; set; } = 3600;

        [DisallowNull, NotNull]
        public string ReadArn { get; set; } = null!;

        [DisallowNull, NotNull]
        public string WriteArn { get; set; } = null!;
    }

    public class AliyunOssOptions : IOptions<AliyunOssOptions>
    {
        public AliyunOssOptions Value
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// 用于存储用户数据的Bucket
        /// </summary>
        [DisallowNull, NotNull]
        public string UserBucketName { get; set; } = null!;

        /// <summary>
        /// 用于公共数据的Bucket
        /// </summary>
        [DisallowNull, NotNull]
        public string PublicBucketName { get; set; } = null!;

        public IList<BucketSettings> Buckets { get; } = new List<BucketSettings>();
    }
}
