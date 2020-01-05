﻿using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using HB.Infrastructure.Aliyun.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Infrastructure.Aliyun.Oss
{
    internal class AliyunOssService : IAliyunOssService
    {
        private const string _rEAD_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\":[\"oss:ListObjects\",\"oss:GetObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";
        private const string _wRITE_ROLE_POLICY_TEMPLATE = "{{ \"Version\":\"1\",\"Statement\":[{{\"Effect\":\"Allow\",\"Action\": [\"oss:DeleteObject\",\"oss:ListParts\",\"oss:AbortMultipartUpload\",\"oss:PutObject\"],\"Resource\":[\"acs:oss:*:*:{0}/*\"]}}]}}";

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
                AliyunUtil.AddEndpoint(ProductNames.STS, settings.RegionId, settings.Sts.Endpoint);

                _acsClients.Add(settings.BucketName, AliyunUtil.CreateAcsClient(settings.RegionId, settings.AccessKeyId, settings.AccessKeySecret));
            }

            _bucketSettings = _options.Buckets.ToDictionary(b => b.BucketName);
        }

        /// <exception cref="AliyunOssException"></exception>
        public AliyunStsToken GetUserDirectoryToken(string bucket, string userGuid, bool isRead)
        {
            return GetDirectoryToken(bucket, GetUserDirectory(bucket, userGuid), GetRoleSessionName(userGuid), isRead);
        }

        //TODO: 等待阿里云出异步方案
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="directory"></param>
        /// <param name="roleSessionName"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        /// <exception cref="AliyunOssException"></exception>
        public AliyunStsToken GetDirectoryToken(string bucket, string directory, string roleSessionName, bool isRead)
        {
            if (!_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                throw new ServiceException($"AliyunOssService went wrong: No bucket found:{bucket}", new AliyunOssException($"No Such Bucket : {bucket}"));
            }

            if (!_acsClients.TryGetValue(bucket, out IAcsClient acsClient))
            {
                throw new ServiceException($"AliyunOssService went wrong: No bucket found:{bucket}", new AliyunOssException($"Can not find AcsClient related to {bucket}"));
            }

            string path = bucket + "/" + directory;
            string policy = isRead ? string.Format(GlobalSettings.Culture, _rEAD_ROLE_POLICY_TEMPLATE, path)
                : string.Format(GlobalSettings.Culture, _wRITE_ROLE_POLICY_TEMPLATE, path);

            AssumeRoleRequest request = new AssumeRoleRequest
            {
                AcceptFormat = FormatType.JSON,
                RoleArn = isRead ? bucketSettings.Sts.ReadArn : bucketSettings.Sts.WriteArn,
                RoleSessionName = roleSessionName,
                DurationSeconds = bucketSettings.Sts.ExpireSeconds,
                Policy = policy
            };

            try
            {
                AssumeRoleResponse assumeRoleResponse = PolicyManager.Default(_logger).Execute(() => acsClient.GetAcsResponse(request));

                AliyunStsToken stsToken = new AliyunStsToken()
                {
                    RequestId = assumeRoleResponse.RequestId,
                    SecurityToken = assumeRoleResponse.Credentials.SecurityToken,
                    AccessKeyId = assumeRoleResponse.Credentials.AccessKeyId,
                    AccessKeySecret = assumeRoleResponse.Credentials.AccessKeySecret,
                    ExpirationAt = assumeRoleResponse.Credentials.Expiration,
                    AssumedRoleId = assumeRoleResponse.AssumedRoleUser.AssumedRoleId,
                    AssumedRoleName = assumeRoleResponse.AssumedRoleUser.Arn
                };

                return stsToken;
            }
            catch (ClientException ex)
            {
                throw new AliyunOssException(Resources.AliyunOssAssumeRoleRequestFailedMessage, ex);
            }
        }

        public string GetOssEndpoint(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.Endpoint;
            }

            throw new ServiceException($"AliyunOssService went wrong: No bucket found:{bucket}", new AliyunOssException($"No Such Bucket : {bucket}"));
        }

        public string GetRegionId(string bucket)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                return bucketSettings.RegionId;
            }

            throw new ServiceException($"AliyunOssService went wrong: No bucket found:{bucket}", new AliyunOssException($"No Such Bucket : {bucket}"));
        }

        private string GetUserDirectory(string bucket, string userGuid)
        {
            if (_bucketSettings.TryGetValue(bucket, out BucketSettings bucketSettings))
            {
                string seprator = bucketSettings.BucketUserDirectory.EndsWith("/", GlobalSettings.Comparison) ? "" : "/";
                return bucketSettings.BucketUserDirectory + seprator + userGuid;
            }

            throw new ServiceException($"AliyunOssService went wrong: No bucket found:{bucket}", new AliyunOssException($"No Such Bucket : {bucket}"));
        }

        private static string GetRoleSessionName(string userGuid)
        {
            return userGuid;
        }
    }
}

