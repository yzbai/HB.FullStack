using HB.Framework.Database;
using System;
using System.Collections.Generic;

namespace HB.Infrastructure.SQLite
{
    public class SQLiteOptions
    {
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();

        public IList<SchemaInfo> Schemas { get; } = new List<SchemaInfo>();

    }
}
