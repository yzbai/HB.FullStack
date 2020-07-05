using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.Framework.Http.Startup
{
    public class JwtSettings
    {
        [DisallowNull, NotNull]
        public string? Authority { get; set; }

        [DisallowNull, NotNull]
        public string? Audience { get; set; }
    }
}
