using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;

using HB.FullStack.Common.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class AliyunStsService : IAliyunStsService
    {
        private const string OSS_WRITE_POLICY_TEMPLATE = @"
{{
    ""Statement"":
    [
        {{
            ""Action"":
            [
                ""oss:ListObjects"", ""oss:GetObject"", ""oss:DeleteObject"",""oss:ListParts"",""oss:AbortMultipartUpload"",""oss:PutObject""
            ],
            ""Effect"": ""Allow"",
            ""Resource"":
            [
                ""acs:oss:*:*:{0}/{1}""
            ]
        }}
    ],
    ""Version"": ""1""
}}";

        private const string OSS_READ_POLICY_TEMPLATE = @"
{{
    ""Statement"":
    [
        {{
            ""Action"":
            [
                ""oss:ListObjects"", ""oss:GetObject""
            ],
            ""Effect"": ""Allow"",
            ""Resource"":
            [
                ""acs:oss:*:*:{0}/{1}""
            ]
        }}
    ],
    ""Version"": ""1""
}}";

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

        public StsToken? RequestOssStsToken(Guid userId, string bucketName, string directory, bool readOnly, double expirySeconds)
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

            string policy = string.Format(
                GlobalSettings.Culture,
                readOnly ? OSS_READ_POLICY_TEMPLATE : OSS_WRITE_POLICY_TEMPLATE,
                bucketName,
                directory.IsNullOrEmpty() ? "*" : directory + "/*");


            //TODO: 如果一秒内有100个请求Sts的，是返回同一个Sts，还是向阿里云请求100个
            //方法：要不公用，要不就ratelimit，可以简单加一个sempha
            //https://help.aliyun.com/document_detail/39744.htm?spm=a2c4g.11186623.0.0.1e0e5d7anw8GO7

            AssumeRoleRequest request = new AssumeRoleRequest
            {
                AcceptFormat = FormatType.JSON,
                RoleArn = assumedRole.Arn,
                RoleSessionName = GetRoleSessionName(userId),
                DurationSeconds = (long)(expirySeconds > assumedRole.MaxExpireSeconds ? assumedRole.MaxExpireSeconds : expirySeconds),
                Policy = policy
            };

            try
            {
                AssumeRoleResponse assumedRoleResponse = _acsClient.GetAcsResponse(request);

                StsToken stsToken = new StsToken
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