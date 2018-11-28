using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.Infrastructure.MongoDB
{
    public class MongoDBSchema
    {
        public string EntityTypeFullName { get; set; }

        public string Database { get; set; }
    }

    public class MongoDBOptions : IOptions<MongoDBOptions>
    {
        public string ConnectionString { get; set; }

        public IList<MongoDBSchema> Schemas { get; set; } = new List<MongoDBSchema>();

        public MongoDBOptions Value => this;

        public string GetDatabaseName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            MongoDBSchema schema = Schemas.First(s => type.FullName.Equals(s.EntityTypeFullName, GlobalSettings.Comparison));

            return schema?.Database;
        }
    }
}
