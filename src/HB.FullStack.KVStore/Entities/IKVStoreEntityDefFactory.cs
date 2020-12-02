using System;

namespace HB.FullStack.KVStore.Entities
{
    internal interface IKVStoreEntityDefFactory
    {
        
        KVStoreEntityDef GetDef<T>();

        
        KVStoreEntityDef GetDef(Type type);
    }
}
