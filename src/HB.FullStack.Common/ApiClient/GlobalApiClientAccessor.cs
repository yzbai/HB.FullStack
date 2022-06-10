using HB.FullStack.Common.ApiClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// //NOTICE:
    /// This class exists because static memeber not supported in interface at .net standard 2.0 and before.
    /// </summary>
    public static class GlobalApiClientAccessor
    {
        public static IApiClient ApiClient { get; internal set; } = null!;
    }
}
