using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class CacheEntityDef
    {
        public PropertyInfo GuidKeyProperty { get; internal set; } = null!;

        public List<PropertyInfo> OtherDimensions { get; private set; } = new List<PropertyInfo>();

        public bool IsCacheable { get; internal set; }

        public string Name { get; internal set; } = null!;

        public DistributedCacheEntryOptions EntryOptions { get; internal set; } = null!;
    }
}
