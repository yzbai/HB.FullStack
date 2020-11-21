using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class CacheEntityDef
    {
        public PropertyInfo GuidKeyProperty { get; internal set; } = null!;

        public List<PropertyInfo> Dimensions { get; private set; } = new List<PropertyInfo>();

        public bool IsCacheable { get; internal set; }

        public string Name { get; internal set; } = null!;

        public TimeSpan? SlidingTime { get; set; }

        public TimeSpan? AbsoluteTimeRelativeToNow { get; set; }

        public string? CacheInstanceName { get; set; }
    }
}
