using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public class ApiResourceDef
    {
        public string EndpointName { get; internal set; } = null!;

        public string ApiVersion { get; internal set; } = null!;

        public string ResourceName { get; internal set; } = null!;

        public string ResourceCollectionName { get; internal set; } = null!;    

        public TimeSpan? RateLimit { get; set; }
    }
}
