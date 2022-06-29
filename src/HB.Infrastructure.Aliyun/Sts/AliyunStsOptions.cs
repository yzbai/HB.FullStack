using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class AssumedRole
    {
        public string Arn { get; set; } = null!;

        public IList<string> Resources { get; set; } = new List<string>();
        
        /// <summary>
        /// 最大有效时间，这个事件后过期
        /// </summary>
        public double MaxExpireSeconds { get; set; } = 60 * 60 * 4;
    }

    public class AliyunStsOptions : IOptions<AliyunStsOptions>
    {
        public string Endpoint { get; set; } = null!;

        /// <summary>
        /// 拥有AliyunSTSAssumeRoleAccess权限的用户
        /// </summary>
        public string UserName { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        public IList<AssumedRole> AssumedRoles { get; set; } = new List<AssumedRole>();

        public AliyunStsOptions Value => this;
    }
}
