using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Session
{
    public class MixedSessionOptions
    {
        public string Name { get; set; } = "DeviceId";
      
        public string CookieDomain { get; set; }

        public string CookiePath { get; set; } = "/";

        public bool CookieHttpOnly { get; set; } = true;

        public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Lax;

        public CookieSecurePolicy CookieSecure { get; set; } = CookieSecurePolicy.None;

        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(20);

        public TimeSpan IOTimeout { get; set; } = TimeSpan.FromMinutes(10);
    }
}
