using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Database;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.Startup
{
    public class InitHostedServiceOptions : IOptions<InitHostedServiceOptions>
    {
        public IList<DbInitContext> DbInitContexts { get; set; } = new List<DbInitContext>();

        public InitHostedServiceOptions Value => this;
    }

    public class DbInitContext
    {
        public string DbSchemaName { get; set; } = null!;

        public IEnumerable<Migration>? Migrations { get; set; }

        /// <summary>
        /// 如果有Migration执行，那么执行Cache清理
        /// </summary>
        public Action<ICache>? CacheCleanAction { get; set; }
    }
}