using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.FullStack.WebApi
{
    public class DataProtectionSettings
    {
        [DisallowNull, NotNull]
        public string ApplicationName { get; set; } = null!;

        public string? CertificateSubject { get; set; }

        public string? CertificateFileName { get; set; }

        public string? CertificateFilePassword { get; set; }

        [DisallowNull, NotNull]
        public string RedisConnectString { get; set; } = null!;
    }
}
