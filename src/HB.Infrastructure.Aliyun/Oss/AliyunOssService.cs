using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Http;
using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    internal class AliyunOssService : IAliyunOssService
    {
        private const string READ_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\":[\"oss:ListObjects\",\"oss:GetObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";
        private const string WRITE_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\": [\"oss:DeleteObject\",\"oss:ListParts\",\"oss:AbortMultipartUpload\",\"oss:PutObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";

        private readonly AliyunOssOptions _options;
        private readonly ILogger _logger;

        private readonly IDictionary<string, IAcsClient> _acsClients;

        private readonly IDictionary<string, BucketSettings> _bucketSettings;

        public string UserBucketName {
            get {
                return _options.UserBucketName;
            }
        }

        public string PublicBucketName {
            get {
                return _options.PublicBucketName;
            }
        }

        public AliyunOssService(IOptions<AliyunOssOptions> options, ILogger<AliyunOssService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _acsClients = new Dictionary<string, IAcsClient>();

            foreach (BucketSettings settings in _options.Buckets)
            {
                AliyunUtil.AddEndpoint(ProductNames.OSS, settings.RegionId, settings.Endpoint);
                AliyunUtil.AddEndpoint(ProductNames.STS, settings.RegionId, settings.Sts.Endpoint);

                _acsClients.Add(settings.BucketName, AliyunUtil.CreateAcsClient(settings.RegionId, settings.AccessKeyId, settings.AccessKeySecret));
            }

            _bucketSettings = _options.Buckets.ToDictionary(b => b.BucketName);
        }

        public Task<AliyunStsToken> GetUserDirectoryTokenAsync(string bucket, string userGuid, bool isRead)
        {
            return GetDirectoryTokenAsync(bucket, GetUserDirectory(bucket, userGuid), GetRoleSessionName(userGuid), isRead);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="directory"></param>
        /// <param name="roleSessionName"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        /// <exception cref="Aliyun.Acs.Core.Exceptions.ServerException"></exception> 
        /// <exception cref="Aliyun.Acs.Core.Exceptions.ClientException"></exception> 
        public Task<AliyunStsToken> GetDirectoryTokenAsync(string bucket, string directory, string roleSessionName, bool isRead)
        {
            string path = bucket + "/" + directory;
            string policy = isRead ? string.Format(GlobalSettings.Culture, READ_ROLE_POLICY_TEMPLATE, path)
                : string.Format(GlobalSettings.Culture, WRITE_ROLE_POLICY_TEMPLATE, path);


            if (!_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return null;
            }

            AssumeRoleRequest request = new AssumeRoleRequest {
                AcceptFormat = FormatType.JSON,
                RoleArn = isRead ? bucketSettings.Sts.ReadArn : bucketSettings.Sts.WriteArn,
                RoleSessionName = roleSessionName,
                DurationSeconds = bucketSettings.Sts.ExpireSeconds,
                Policy = policy
            };

            if (_acsClients.TryGetValue(bucket, out IAcsClient acsClient))
            {
                return PolicyManager.Default(_logger).ExecuteAsync(async () => {
                    Task<AssumeRoleResponse> task = new Task<AssumeRoleResponse>(() => acsClient.GetAcsResponse(request));
                    task.Start(TaskScheduler.Default);

                    AssumeRoleResponse response = await task.ConfigureAwait(false);

                    AliyunStsToken stsToken = new AliyunStsToken() {
                        RequestId = response.RequestId,
                        SecurityToken = response.Credentials.SecurityToken,
                        AccessKeyId = response.Credentials.AccessKeyId,
                        AccessKeySecret = response.Credentials.AccessKeySecret,
                        ExpirationAt = response.Credentials.Expiration,
                        AssumedRoleId = response.AssumedRoleUser.AssumedRoleId,
                        AssumedRoleName = response.AssumedRoleUser.Arn
                    };

                    return stsToken;
                });
            }

            return null;
        }

        public string GetOssEndpoint(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.Endpoint;
            }

            return null;
        }

        public string GetRegionId(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.RegionId;
            }

            return null;
        }

        private string GetUserDirectory(string bucket, string userGuid)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                string seprator = bucketSettings.BucketUserDirectory.EndsWith("/", GlobalSettings.Comparison) ? "" : "/";
                return  bucketSettings.BucketUserDirectory + seprator + userGuid;
            }

            return null;
        }

        private static string GetRoleSessionName(string userGuid)
        {
            return userGuid;
        }
    }
}

