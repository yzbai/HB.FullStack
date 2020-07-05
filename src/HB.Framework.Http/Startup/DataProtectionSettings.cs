using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.Framework.Http.Startup
{
    public class DataProtectionSettings
    {
        [DisallowNull, NotNull]
        public string? ApplicationDiscriminator { get; set; }

        [DisallowNull, NotNull]
        public string? CertificateSubject { get; set; }

        [DisallowNull, NotNull]
        public string? RedisConnectString { get; set; }
    }
}
