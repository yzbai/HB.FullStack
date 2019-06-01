using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Entity
{
    internal interface IKVStoreEntityDefFactory
    {
        KVStoreEntityDef GetDef<T>();
        KVStoreEntityDef GetDef(Type type);
    }
}
