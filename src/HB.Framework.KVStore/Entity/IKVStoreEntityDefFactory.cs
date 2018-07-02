using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Entity
{
    public interface IKVStoreEntityDefFactory
    {
        KVStoreEntityDef Get<T>();
        KVStoreEntityDef Get(Type type);
    }
}
