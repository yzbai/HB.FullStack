using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.MySQL
{
    public class MySQLDatabaseSetting
    {
        public bool IsMaster { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MySQLEngineOptions : IOptions<MySQLEngineOptions>
    {
        public MySQLEngineOptions Value { get { return this; } }

        public IList<MySQLDatabaseSetting> DatabaseSettings { get; set; }

        public MySQLEngineOptions()
        {
            DatabaseSettings = new List<MySQLDatabaseSetting>();
        }

        public IEnumerable<MySQLDatabaseSetting> GetDatabaseSetting(string databaseName)
        {
            return DatabaseSettings.Where(ds => ds.DatabaseName.Equals(databaseName, GlobalSettings.ComparisonIgnoreCase));
        }
    }
}
