using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.Infrastructure.Aliyun.Sms
{
    public class AliyunSmsOptions : IOptions<AliyunSmsOptions>
    {
        public AliyunSmsOptions Value { get { return this; } }

        [DisallowNull,NotNull]
        public string? RegionId { get; set; }

        [DisallowNull, NotNull]
        public string? Endpoint { get; set; }

        [DisallowNull, NotNull]
        public string? AccessUserName { get; set; }

        [DisallowNull, NotNull]
        public string? AccessKeyId { get; set; }

        [DisallowNull, NotNull]
        public string? AccessKeySecret { get; set; }

        [DisallowNull, NotNull]
        public string? SignName { get; set; }

        public TemplateIdentityValidation TemplateIdentityValidation { get; set; } = new TemplateIdentityValidation();
    }

    public class TemplateIdentityValidation
    {
        [DisallowNull, NotNull]
        public string? TemplateCode { get; set; }

        [DisallowNull, NotNull]
        public string? ParamProduct { get; set; }

        [DisallowNull, NotNull]
        public string? ParamCode { get; set; }

        [DisallowNull, NotNull]
        public int CodeLength { get; set; } = 6;

        [DisallowNull, NotNull]
        public string? ParamProductValue { get; set; }

        [DisallowNull, NotNull]
        public int ExpireMinutes { get; set; } = 10;
    }
}
