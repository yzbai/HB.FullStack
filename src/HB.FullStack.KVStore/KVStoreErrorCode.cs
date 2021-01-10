namespace HB.FullStack.KVStore
{
    public enum KVStoreErrorCode
    {
        CacheLoadedLuaNotFound,
        KVStoreRedisConnectionFailed,
        KVStoreRedisTimeout,
        KVStoreError,
        NoSuchInstance,
        KVStoreExistAlready,
        KVStoreVersionNotMatched,
        OK,
        KVStoreNoEntitySchemaFound,
        LackKVStoreKeyAttributeErrorMessage,
        VersionsKeysNotEqualErrorMessage
    }
}