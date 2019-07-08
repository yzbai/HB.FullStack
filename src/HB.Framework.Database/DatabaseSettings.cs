using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database
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
}
