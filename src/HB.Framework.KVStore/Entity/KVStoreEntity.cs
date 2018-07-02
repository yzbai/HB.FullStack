using HB.Framework.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Entity
{
    [Serializable]
    public class KVStoreEntity : CommonEntity
    {
        public int Version { get; set; } = 0;
    }
}
