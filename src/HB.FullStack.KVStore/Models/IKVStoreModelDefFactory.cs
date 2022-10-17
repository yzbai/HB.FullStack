using System;

using HB.FullStack.KVStore.Engine;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public interface IKVStoreModelDefFactory
    {
        KVStoreModelDef GetDef<T>();
        KVStoreModelDef GetDef(Type type);
        void Initialize(IKVStoreEngine kVStoreEngine);
    }
}