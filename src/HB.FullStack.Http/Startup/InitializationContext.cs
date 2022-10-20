using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

namespace System
{
    public class InitializationContext
    {
        public IEnumerable<Migration>? Migrations { get; set; }

        public Action<ICache>? CacheCleanAction { get; set; }
    }
}