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
    public class AliyunStsService : IAliyunStsService
    {
        private const string _oSS_WRITE_POLICY_TEMPLATE = "{{\"Statement\": [{{\"Action\": [\"oss:ListObjects\", \"oss:GetObject\", \"oss:DeleteObject\",\"oss:ListParts\",\"oss:AbortMultipartUpload\",\"oss:PutObject\"],\"Effect\": \"Allow\",\"Resource\": [\"acs:oss:*:*:{0}/{1}\"]}}],\"Version\": \"1\"}}";
        private const string _oSS_READ_POLICY_TEMPLATE = "{{\"Statement\": [{{\"Action\": [\"oss:ListObjects\", \"oss:GetObject\"],\"Effect\": \"Allow\",\"Resource\": [\"acs:oss:*:*:{0}/{1}\"]}}],\"Version\": \"1\"}}";

        private readonly AliyunStsOptions _options;
        private readonly IAcsClient _acsClient;
        private readonly Dictionary<string, AssumedRole> _resourceAssumedRoleDict = new Dictionary<string, AssumedRole>();

        public AliyunStsService(IOptions<AliyunStsOptions> options)
        {
            _options = options.Value;

            AliyunUtil.AddEndpoint(AliyunProductNames.STS, "", _options.Endpoint);
            _acsClient = AliyunUtil.CreateAcsClient("", _options.AccessKeyId, _options.AccessKeySecret);

            foreach (AssumedRole assumedRole in _options.AssumedRoles)
            {
                foreach (string resource in assumedRole.Resources)
                {
                    _resourceAssumedRoleDict[resource] = assumedRole;
                }
            }
        }

        private static string GetRoleSessionName(Guid userId)
        {
            return "User" + userId.ToString();
        }

        public AliyunStsToken? RequestOssStsToken(Guid userId, string bucketName, string directory, bool readOnly)
        {
            if (bucketName.IsNullOrEmpty() || userId.IsEmpty() || directory.IsNullOrEmpty())
            {
                return null;
            }

            directory = directory.TrimEnd('/');

            string ossResourceName = $"acs:oss:*:*:{bucketName}";

            if (!_resourceAssumedRoleDict.TryGetValue(ossResourceName, out AssumedRole? assumedRole))
            {
                return null;
            }

            string policy = string.Format(GlobalSettings.Culture, readOnly ? _oSS_READ_POLICY_TEMPLATE : _oSS_WRITE_POLICY_TEMPLATE, bucketName, directory.IsNullOrEmpty() ? "*" : directory + "/*");

            AssumeRoleRequest request = new AssumeRoleRequest
            {
                AcceptFormat = FormatType.JSON,
                RoleArn = assumedRole.Arn,
                RoleSessionName = GetRoleSessionName(userId),
                DurationSeconds = assumedRole.ExpireSeconds,
                Policy = policy
            };

            try
            {
                AssumeRoleResponse assumedRoleResponse = _acsClient.GetAcsResponse(request);

                AliyunStsToken stsToken = new AliyunStsToken
                {
                    RequestId = assumedRoleResponse.RequestId,
                    SecurityToken = assumedRoleResponse.Credentials.SecurityToken,
                    AccessKeyId = assumedRoleResponse.Credentials.AccessKeyId,
                    AccessKeySecret = assumedRoleResponse.Credentials.AccessKeySecret,
                    ExpirationAt = DateTimeOffset.Parse(assumedRoleResponse.Credentials.Expiration, GlobalSettings.Culture),
                    ArId = assumedRoleResponse.AssumedRoleUser.AssumedRoleId,
                    Arn = assumedRoleResponse.AssumedRoleUser.Arn,
                    ReadOnly = readOnly
                };

                return stsToken;
            }
            catch (Exception ex)
            {
                //TODO: 处理报错
                throw AliyunExceptions.StsError(userId: userId, bucketname: bucketName, direcotry: directory, readOnly: readOnly, ex);
            }
        }
    }
}