using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class AliyunStsToken
    {
        public string RequestId { get; set; }

        public string SecurityToken { get; set; }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        public DateTimeOffset ExpirationAt { get; set; }

        public string AssumedRoleId { get; set; }

        public string AssumedRoleName { get; set; }

        public IList<string> Resources { get; set; } = new List<string>();

        public AliyunStsToken(string requestId, string securityToken, string accessKeyId, string accessKeySecret, string expirationAt, string assumedRoleId, string assumedRoleName, string[] resources)
        {
            RequestId = requestId;
            SecurityToken = securityToken;
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            ExpirationAt = DateTimeOffset.Parse(expirationAt, GlobalSettings.Culture);
            AssumedRoleId = assumedRoleId;
            AssumedRoleName = assumedRoleName;
            Resources = resources;
        }

    }
}
