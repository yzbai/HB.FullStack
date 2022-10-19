using System.Collections.Generic;

using HB.FullStack.Database;

using Microsoft.Extensions.Options;

namespace HB.Infrastructure.MySQL
{
    public class MySQLOptions : IOptions<MySQLOptions>
    {
        public DbCommonSettings CommonSettings { get; set; } = new DbCommonSettings();

        public IList<DbConnectionSettings> Connections { get; } = new List<DbConnectionSettings>();

        public MySQLOptions Value => this;
    }
}