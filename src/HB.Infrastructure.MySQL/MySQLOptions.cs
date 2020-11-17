using HB.Framework.Database;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace HB.Infrastructure.MySQL
{
    public class MySQLOptions : IOptions<MySQLOptions>
    {
        public DatabaseCommonSettings CommonSettings { get; set; } = new DatabaseCommonSettings();

        public IList<DatabaseConnectionSettings> Connections { get; } = new List<DatabaseConnectionSettings>();

        public MySQLOptions Value => this;
    }
}