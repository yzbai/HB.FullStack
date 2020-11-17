using System.Collections.Generic;
using HB.Framework.Database;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteOptions : IOptions<SQLiteOptions>
    {
        public DatabaseCommonSettings CommonSettings { get; set; } = new DatabaseCommonSettings();

        public IList<DatabaseConnectionSettings> Connections { get; private set; } = new List<DatabaseConnectionSettings>();

        public SQLiteOptions Value => this;
    }
}