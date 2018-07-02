using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.Identity
{
    public class IdentityOptions : IOptions<IdentityOptions>
    {
        public IdentityOptions Value { get { return this; } }


        public IdentityEvents Events { get; set; }
    }
}
