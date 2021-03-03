using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Infrastructure.Aliyun.Oss
{
    internal class AliyunOssService : IAliyunOssService
    {
#pragma warning disable CA1823 // Avoid unused private fields
        private const string READ_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\":[\"oss:ListObjects\",\"oss:GetObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";
        private const string WRITE_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\": [\"oss:DeleteObject\",\"oss:ListParts\",\"oss:AbortMultipartUpload\",\"oss:PutObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";
#pragma warning restore CA1823 // Avoid unused private fields

        private readonly AliyunOssOptions _options;
        private readonly ILogger _logger;

        private readonly IDictionary<string, IAcsClient> _acsClients;
        private readonly IDictionary<string, BucketSettings> _bucketSettings;

        public string UserBucketName { get { return _options.UserBucketName; } }

        public string PublicBucketName { get { return _options.PublicBucketName; } }

        public AliyunOssService(IOptions<AliyunOssOptions> options, ILogger<AliyunOssService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _acsClients = new Dictionary<string, IAcsClient>();

            foreach (BucketSettings settings in _options.Buckets)
            {
                AliyunUtil.AddEndpoint(ProductNames.OSS, settings.RegionId, settings.Endpoint);
               

                _acsClients.Add(settings.BucketName, AliyunUtil.CreateAcsClient(settings.RegionId, settings.AccessKeyId, settings.AccessKeySecret));
            }

            _bucketSettings = _options.Buckets.ToDictionary(b => b.BucketName);
        }

 

         
        /// <summary>
        /// GetOssEndpoint
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        /// <exception cref="AliyunException"></exception>
        public string GetOssEndpoint(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.Endpoint;
            }

            throw new AliyunException( AliyunErrorCode.OssError,$"No Such Bucket : {bucket}");
        }

        /// <summary>
        /// GetRegionId
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        /// <exception cref="AliyunException"></exception>
        public string GetRegionId(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.RegionId;
            }

            throw new AliyunException( AliyunErrorCode.OssError,$"No Such Bucket : {bucket}");
        }

        /// <summary>
        /// GetUserDirectory
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        /// <exception cref="AliyunException"></exception>
        private string GetUserDirectory(string bucket, string userGuid)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                string seprator = bucketSettings.BucketUserDirectory.EndsWith("/", GlobalSettings.Comparison) ? "" : "/";
                return bucketSettings.BucketUserDirectory + seprator + userGuid;
            }

            throw new AliyunException( AliyunErrorCode.OssError,$"No Such Bucket : {bucket}");
        }

        private static string GetRoleSessionName(string userGuid)
        {
            return userGuid;
        }
    }
}

