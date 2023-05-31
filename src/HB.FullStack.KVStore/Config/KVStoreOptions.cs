using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;

namespace HB.FullStack.KVStore.Config
{
    public class KVStoreOptions : IOptions<KVStoreOptions>
    {
        public KVStoreOptions Value => this;
        
        public string? ApplicationName { get; set; }

        public IList<KVStoreSchema> KVStoreSchemas { get; set; } = new List<KVStoreSchema>();

        public IList<string> KVStoreModelAssemblies { get; } = new List<string>();


    }
}
