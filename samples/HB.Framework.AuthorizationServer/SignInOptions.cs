using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.AuthorizationServer
{
    public class SignInOptions
    {
        public TimeSpan RefreshTokenLongExpireTimeSpan { get; set; } = TimeSpan.FromDays(365);
        public TimeSpan RefreshTokenShortExpireTimeSpan { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan AccessTokenExpireTimeSpan { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan LockoutTimeSpan { get; set; } = TimeSpan.FromHours(6);
        public bool RequiredMaxFailedCountCheck { get; set; } = false;
        public bool RequiredLockoutCheck { get; set; } = false;
        public bool RequireEmailConfirmed { get; set; } = false;
        public bool RequireMobileConfirmed { get; set; } = false;
        public bool RequireTwoFactorCheck { get; set; } = false;
        public long MaxFailedCount { get; set; } = 4;
        public double AccessFailedRecoveryDays { get; set; } = 1;
        public long LockoutAfterAccessFailedCount { get; set; } = 4;
        public bool AllowOnlyOneAppClient { get; set; } = true;
    }
}
