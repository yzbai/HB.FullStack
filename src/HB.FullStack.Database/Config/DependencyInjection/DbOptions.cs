using System.Collections.Generic;

using HB.FullStack.Database;
using HB.FullStack.Database.Config;

using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Database模块总配置
    /// </summary>
    public class DbOptions : IOptions<DbOptions>
    {
        public DbOptions Value => this;

        public IList<DbSchema> DbSchemas { get; set; } = new List<DbSchema>();

        /// <summary>
        /// 指定包含DbModel的Assemblies，不再去其他Assembly中查找
        /// </summary>
        public IList<string> DbModelAssemblies { get; set; } = new List<string>();

        public IList<DbInitContext> InitContexts { get; set; } = new List<DbInitContext>();
    }
}