using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HB.FullStack.KVStore.Config
{
    //TODO: 考虑KVStoreModel的Migration

    public class KVStoreSchema
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; } = null!;

        public ConnectionString ConnectionString { get; set; } = null!;


        public IList<string> ModelTypeFullNames = new List<string>();       

    }
}