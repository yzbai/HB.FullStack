using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Microsoft.Extensions.Options;

namespace HB.Infrastructure.Aliyun.Sts
{
    public class StsSetting
    {
        public IList<string> ResourceNames { get; set; } = new List<string>();

        [DisallowNull, NotNull]
        public string Arn { get; set; } = null!;

        [DisallowNull, NotNull]
        public string RoleSessionName { get; set; } = null!;
        
        public int ExpireSeconds { get; set; } = 3600;

        public string? RolePolicy { get; set; } 
    }

    public class AliyunStsOptions : IOptions<AliyunStsOptions>
    {
        [DisallowNull, NotNull]
        public string Endpoint { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeyId { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeySecret { get; set; } = null!;

        public IList<StsSetting> StsSettings { get; set; } = new List<StsSetting>();

        public AliyunStsOptions Value => this;
    }
}
