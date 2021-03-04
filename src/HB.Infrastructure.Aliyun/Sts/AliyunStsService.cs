using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Infrastructure.Aliyun.Sts
{
    internal class AliyunStsService : IAliyunStsService
    {
        private readonly AliyunStsOptions _options;
        private readonly ILogger _logger;

        private readonly IAcsClient _acsClient;


        private readonly string _rolePolicyTemplate = "{\"Statement\": [{\"Action\": \"oss:*\",\"Effect\": \"Allow\",\"Resource\": [\"acs:oss:*:*:mycolorfultime-private-dev\",\"acs:oss:*:*:mycolorfultime-private-dev/*\"]}],\"Version\": \"1\"}";

        public AliyunStsService(IOptions<AliyunStsOptions> options, ILogger<AliyunStsService> logger)
        {
            _options = options.Value;
            _logger = logger;

            AliyunUtil.AddEndpoint(ProductNames.STS, "", _options.Endpoint);
            _acsClient = AliyunUtil.CreateAcsClient("", _options.AccessKeyId, _options.AccessKeySecret);
        }

        private static string GetRoleSessionName(long userId)
        {
            return "User" + userId;
        }

        /// <exception cref="AliyunException"></exception>
        public AliyunStsToken? GetStsToken(string resource, long userId)
        {
            StsSetting? stsSetting = _options.StsSettings.SingleOrDefault(s => s.ResourceNames.Contains(resource));

            if (stsSetting == null)
            {
                return null;
            }

            AssumeRoleRequest request = new AssumeRoleRequest
            {
                AcceptFormat = FormatType.JSON,
                RoleArn = stsSetting.Arn,
                RoleSessionName = GetRoleSessionName(userId),
                DurationSeconds = stsSetting.ExpireSeconds,
                Policy = stsSetting.RolePolicy
            };

            try
            {
                AssumeRoleResponse assumeRoleResponse = _acsClient.GetAcsResponse(request);

                AliyunStsToken stsToken = new AliyunStsToken(
                    assumeRoleResponse.RequestId,
                    assumeRoleResponse.Credentials.SecurityToken,
                    assumeRoleResponse.Credentials.AccessKeyId,
                    assumeRoleResponse.Credentials.AccessKeySecret,
                    assumeRoleResponse.Credentials.Expiration,
                    assumeRoleResponse.AssumedRoleUser.AssumedRoleId,
                    assumeRoleResponse.AssumedRoleUser.Arn,
                    stsSetting.ResourceNames.ToArray()
                );

                return stsToken;
            }
            catch (Exception ex)
            {
                //TODO: 处理报错
                throw new AliyunException(AliyunErrorCode.OssError, $"AliyunOssAssumeRoleRequestFailed", ex);
            }
        }
    }
}

