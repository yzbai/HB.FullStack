using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.FullStack.Server
{
    public class DataProtectionSettings
    {
        [DisallowNull, NotNull]
        public string ApplicationName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string CertificateSubject { get; set; } = null!;

        [DisallowNull, NotNull]
        public string RedisConnectString { get; set; } = null!;
    }
}
