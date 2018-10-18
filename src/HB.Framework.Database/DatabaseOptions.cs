using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace HB.Framework.Database
{
    public class DatabaseSchema
    {
        public string Assembly { get; set; }
        public string EntityTypeFullName { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Description { get; set; }
        public bool Writeable { get; set; }
    }

    public class DatabaseOptions : IOptions<DatabaseOptions>
    {
        public DatabaseOptions Value { get { return this; } }

        public int DefaultVarcharLength { get; set; } = 144;

        public IList<DatabaseSchema> DatabaseSchemas { get; set; }

        public DatabaseOptions()
        {
            DatabaseSchemas = new List<DatabaseSchema>();
        }

        public DatabaseSchema GetDatabaseSchema(string modelTypeFullName)
        {
            return DatabaseSchemas.First(ds => ds.EntityTypeFullName.Equals(modelTypeFullName, StringComparison.InvariantCulture));
        }
    }
}
