﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.FullStack.Server
{
    public class JwtClientSettings
    {
        [DisallowNull, NotNull]
        public string Authority { get; set; } = null!;

        [DisallowNull, NotNull]
        public string Audience { get; set; } = null!;

        /// <summary>
        /// 要与AuthorizationServerOptions中的EncryptingCertificateSubject相同
        /// </summary>
        [DisallowNull, NotNull]
        public string DecryptionCertificateSubject { get; set; } = null!;
    }
}
