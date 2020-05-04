using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Aliyun
{
    public class AliyunStsToken
    {
        public string RequestId { get; set; }

        public string SecurityToken { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public string ExpirationAt { get; set; }

        public string AssumedRoleId { get; set; }

        public string AssumedRoleName { get; set; }

        public AliyunStsToken(string requestId, string securityToken, string accessKeyId, string accessKeySecret, string expirationAt, string assumedRoleId, string assumedRoleName)
        {
            RequestId = requestId;
            SecurityToken = securityToken;
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            ExpirationAt = expirationAt;
            AssumedRoleId = assumedRoleId;
            AssumedRoleName = assumedRoleName;
        }

    }
}
