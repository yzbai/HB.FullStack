namespace HB.Framework.KVStore
{
    public enum KVStoreError
    {
        InnerError = 0,
        Succeeded = 1,
        NotFound = 2,
        //Failed,
        ExistAlready = 3,
        VersionNotMatched = 4,
        NotValid = 5,
        UnKown = 6,
        NoEntitySchemaFound = 7,
        RedisTimeout = 8,
        RedisConnectionFailed = 9
    }
}
