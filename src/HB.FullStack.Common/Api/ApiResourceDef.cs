using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public class ApiResourceDef
    {
        public string EndpointName { get; internal set; } = null!;

        public string ApiVersion { get; internal set; } = null!;

        public string ResName { get; internal set; } = null!;

     
    }
}
