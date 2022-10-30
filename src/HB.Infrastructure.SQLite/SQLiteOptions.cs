using System.Collections.Generic;
using HB.FullStack.Database;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteOptions : IOptions<SQLiteOptions>
    {
        public DbSetting CommonSettings { get; set; } = new DbSetting();

        public IList<DbConnectionSetting> Connections { get; private set; } = new List<DbConnectionSetting>();

        public SQLiteOptions Value => this;
    }
}