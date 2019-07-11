using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.Framework.KVStore
{
    public class KVStoreSchema
    {
        public string EntityTypeFullName { get; set; }
        public string InstanceName { get; set; }
        public string Description { get; set; } = "";
    }

    public class KVStoreOptions : IOptions<KVStoreOptions>
    {
        public KVStoreOptions Value { get { return this; } }

        public IList<KVStoreSchema> KVStoreSchemas { get; } = new List<KVStoreSchema>();


        public KVStoreSchema GetKVStoreSchema(string entityTypeFullName)
        {
            return KVStoreSchemas.FirstOrDefault<KVStoreSchema>(ks => ks.EntityTypeFullName.Equals(entityTypeFullName, GlobalSettings.Comparison));
        }
    }
}