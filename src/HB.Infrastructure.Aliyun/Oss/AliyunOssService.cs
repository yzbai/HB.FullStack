using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Http;
using HB.Infrastructure.Aliyun.Sts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Oss
{
    public class AliyunOssService : IAliyunOssService
    {
        private static readonly string READ_ROLE_POLICY_TEMPLATE = "{ \"Version\":\"1\",\"Statement\":[{\"Effect\":\"Allow\",\"Action\":[\"oss:ListObjects\",\"oss:GetObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}]}";
        private static readonly string WRITE_ROLE_POLICY_TEMPLATE = "{ \"Version\":\"1\",\"Statement\":[{\"Effect\":\"Allow\",\"Action\": [\"oss:DeleteObject\",\"oss:ListParts\",\"oss:AbortMultipartUpload\",\"oss:PutObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}]}";

        private AliyunOssOptions _options;
        private IAcsClient _acsClient;
        private ILogger _logger;

        public AliyunOssService(IOptions<AliyunOssOptions> options, IAcsClientManager acsClientManager, ILogger<AliyunOssService> logger)
        {
            _options = options.Value;
            _acsClient = acsClientManager.GetAcsClient(_options.ProductName);
            _logger = logger;
        }

        public Task<StsRoleCredential> GetUserReadRoleCredentialAsync(string bucket, string userGuid)
        {
            return GetDirectoryReadRoleCredentialAsync(bucket, GetUserDirectory(bucket, userGuid), GetRoleSessionName(bucket, userGuid));
        }


        public Task<StsRoleCredential> GetUserWriteRoleCredentialAsync(string bucket, string userGuid)
        {
            return GetDirectoryWriteRoleCredentialAsync(bucket, GetUserDirectory(bucket, userGuid), GetRoleSessionName(bucket, userGuid));
        }

        public Task<StsRoleCredential> GetDirectoryReadRoleCredentialAsync(string bucket, string directory, string roleSessionName)
        {
            string path = bucket + "/" + directory;
            string policy = string.Format(GlobalSettings.Culture, READ_ROLE_POLICY_TEMPLATE, path);

            BucketSettings bucketSettings = _options.GetBucketSettings(bucket);

            return GetStsRoleCredentialAsync(bucketSettings.ReadArn, roleSessionName, policy, bucketSettings.StsExpireSeconds);
        }

        public Task<StsRoleCredential> GetDirectoryWriteRoleCredentialAsync(string bucket, string directory, string roleSessionName)
        {

            string path = bucket + "/" + directory;
            string policy = string.Format(GlobalSettings.Culture, WRITE_ROLE_POLICY_TEMPLATE, path);

            BucketSettings bucketSettings = _options.GetBucketSettings(bucket);

            return GetStsRoleCredentialAsync(bucketSettings.ReadArn, roleSessionName, policy, bucketSettings.StsExpireSeconds);
        }

        private Task<StsRoleCredential> GetStsRoleCredentialAsync(string roleArn, string roleSessionName, string policy, int expireSeconds = 3600)
        {
            AssumeRoleRequest request = new AssumeRoleRequest();

            request.AcceptFormat = FormatType.JSON;
            request.RoleArn = roleArn;
            request.RoleSessionName = roleSessionName;
            request.DurationSeconds = expireSeconds;
            request.Policy = policy;

            return PolicyManager.Default(_logger).ExecuteAsync(async ()=> {
                Task<AssumeRoleResponse> task = new Task<AssumeRoleResponse>(()=>_acsClient.GetAcsResponse(request));
                task.Start(TaskScheduler.Default);

                AssumeRoleResponse response = await task.ConfigureAwait(false);

                StsRoleCredential credential = new StsRoleCredential() {
                    RequestId = response.RequestId,
                    SecurityToken = response.Credentials.SecurityToken,
                    AccessKeyId = response.Credentials.AccessKeyId,
                    AccessKeySecret = response.Credentials.AccessKeySecret,
                    Expiration = response.Credentials.Expiration,
                    AssumedRoleId = response.AssumedRoleUser.AssumedRoleId,
                    AssumedRoleName = response.AssumedRoleUser.Arn
                };

                return credential;
            });
        }

        private string GetUserDirectory(string bucket, string userGuid)
        {
            BucketSettings bucketSettings = _options.GetBucketSettings(bucket);

            string seprator = bucketSettings.UserDirectoryPath.EndsWith("/", GlobalSettings.Comparison) ? "" : "/";
            string directory = bucketSettings.UserDirectoryPath + seprator + userGuid;
            return directory;
        }

        private static string GetRoleSessionName(string bucket, string userGuid)
        {
            return $"{bucket}-{userGuid}";
        }
    }
}

