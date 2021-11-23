using HB.FullStack.Common.ApiClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class GlobalDefaultApiClientAccessor
    {
        public static IApiClient ApiClient { get; internal set; } = null!;
    }
}
