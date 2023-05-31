using System;

using HB.FullStack.KVStore.Engine;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public interface IKVStoreModelDefFactory
    {
        KVStoreModelDef GetDef<T>() where T : class, IKVStoreModel;
        KVStoreModelDef? GetDef(Type type);
    }
}