using System.Collections.Generic;
using HB.FullStack.Database;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteOptions : IOptions<SQLiteOptions>
    {
        public DbCommonSettings CommonSettings { get; set; } = new DbCommonSettings();

        public IList<DbConnectionSettings> Connections { get; private set; } = new List<DbConnectionSettings>();

        public SQLiteOptions Value => this;
    }
}