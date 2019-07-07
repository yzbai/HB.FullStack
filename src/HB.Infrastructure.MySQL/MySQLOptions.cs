using System;
using System.Collections.Generic;
using System.Linq;
using HB.Framework.Database;

namespace HB.Infrastructure.MySQL
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public int Version { get; set; }

        public int DefaultVarcharLength { get; set; } = 200;

        public IList<EntitySchema> Entities { get; } = new List<EntitySchema>();

        public bool AutomaticCreateTable => true;
    }

    public class SchemaInfo
    {
        public bool IsMaster { get; set; }
        public string SchemaName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MySQLOptions
    {
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();

        public IList<SchemaInfo> Schemas { get; } = new List<SchemaInfo>();

    }
}
