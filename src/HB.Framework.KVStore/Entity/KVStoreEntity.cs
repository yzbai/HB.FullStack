using HB.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Entity
{
    public class KVStoreEntity : ValidatableObject
    {
        public int Version { get; set; } = 0;
    }
}
