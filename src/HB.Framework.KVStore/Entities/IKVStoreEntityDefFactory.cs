﻿using System;

namespace HB.Framework.KVStore.Entities
{
    internal interface IKVStoreEntityDefFactory
    {
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        KVStoreEntityDef GetDef<T>();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        KVStoreEntityDef GetDef(Type type);
    }
}