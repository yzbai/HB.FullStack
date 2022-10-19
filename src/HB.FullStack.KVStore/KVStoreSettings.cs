using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HB.FullStack.KVStore
{
    public class KVStoreModelSchema
    {
        [DisallowNull, NotNull]
        public string ModelTypeFullName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string InstanceName { get; set; } = null!;

        public string? Description { get; set; }
    }

    public class KVStoreSettings
    {
        public IList<string> AssembliesIncludeModel { get; } = new List<string>();

        public IList<KVStoreModelSchema> KVStoreModels { get; } = new List<KVStoreModelSchema>();

    }
}