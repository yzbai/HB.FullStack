using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public class IdentityEvents
    {
        public Func<IdentitySecurityStampChangeContext, Task> OnSecurityStampChanged { get; set; } = context => Task.CompletedTask;

        public virtual Task SecurityStampChanged(IdentitySecurityStampChangeContext context)
        {
            return OnSecurityStampChanged(context);
        }
    }
}
