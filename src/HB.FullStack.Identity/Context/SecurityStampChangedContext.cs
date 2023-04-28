using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Server.Identity
{
    public class SecurityStampChangedContext
    {
        public long UserId { get; set; }

        public SecurityStampChangedContext(long userId)
        {
            UserId = userId;
        }
    }
}
