using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

namespace System
{
    public class InitializationContext
    {
        public IList<DbInitializeContext> DbInitializeContexts { get; set; } = new List<DbInitializeContext>();
    }

    public class DbInitializeContext
    {
        public string DbSchema { get; set; } = null!;

        public IEnumerable<Migration>? Migrations { get; set; }

        /// <summary>
        /// 如果有Migration执行，那么执行Cache清理
        /// </summary>
        public Action<ICache>? CacheCleanAction { get; set; }
    }
}