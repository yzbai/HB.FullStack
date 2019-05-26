using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentitySecurityStampChangeContext
    {
        public string UserGuid { get; set; }

        public IdentitySecurityStampChangeContext(string userGuid)
        {
            UserGuid = userGuid;
        }
    }
}
