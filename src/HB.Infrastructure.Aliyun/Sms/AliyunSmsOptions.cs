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

        [DisallowNull, NotNull]
        public string RegionId { get; set; } = null!;

        [DisallowNull, NotNull]
        public string Endpoint { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessLoginName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeyId { get; set; } = null!;

        [DisallowNull, NotNull]
        public string AccessKeySecret { get; set; } = null!;

        [DisallowNull, NotNull]
        public string SignName { get; set; } = null!;

        public TemplateIdentityValidation TemplateIdentityValidation { get; set; } = new TemplateIdentityValidation();
    }

    public class TemplateIdentityValidation
    {
        [DisallowNull, NotNull]
        public string TemplateCode { get; set; } = null!;

        [DisallowNull, NotNull]
        public string ParamProduct { get; set; } = null!;

        [DisallowNull, NotNull]
        public string ParamCode { get; set; } = null!;

        [DisallowNull, NotNull]
        public int CodeLength { get; set; } = 6;

        [DisallowNull, NotNull]
        public string ParamProductValue { get; set; } = null!;

        [DisallowNull, NotNull]
        public int ExpireMinutes { get; set; } = 2;
    }
}
