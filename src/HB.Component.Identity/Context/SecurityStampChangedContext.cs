using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Identity
{
    public class SecurityStampChangedContext
    {
        public string UserGuid { get; set; }

        public SecurityStampChangedContext(string userGuid)
        {
            UserGuid = userGuid;
        }
    }
}
