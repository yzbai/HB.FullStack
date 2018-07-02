using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentitySecurityStampChangeContext
    {
        public long UserId { get; set; }

        public IdentitySecurityStampChangeContext(long userId)
        {
            UserId = userId;
        }
    }
}
