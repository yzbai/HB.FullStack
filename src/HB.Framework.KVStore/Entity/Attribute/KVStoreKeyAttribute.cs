using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Entity
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KVStoreKeyAttribute : System.Attribute
    {
        public int Order { get; set; } = 0;
    }
}
