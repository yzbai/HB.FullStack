using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

namespace System
{
    public class InitializationOptions : IOptions<InitializationOptions>
    {
        public IList<DbInitializeContext> DbInitializeContexts { get; set; } = new List<DbInitializeContext>();

        public InitializationOptions Value => this;
    }

    public class DbInitializeContext
    {
        public string DbSchemaName { get; set; } = null!;

        public IEnumerable<Migration>? Migrations { get; set; }

        /// <summary>
        /// 如果有Migration执行，那么执行Cache清理
        /// </summary>
        public Action<ICache>? CacheCleanAction { get; set; }
    }
}