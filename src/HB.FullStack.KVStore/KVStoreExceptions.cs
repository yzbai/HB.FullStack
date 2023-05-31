using System;

namespace HB.FullStack.KVStore
{
    public static class KVStoreExceptions
    {
        public static Exception NoModelSchemaFound(string? type)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.NoModelSchemaFound, nameof(NoModelSchemaFound));

            exception.Data["Type"] = type;

            return exception;
        }

        public static Exception LackKVStoreKeyAttributeError(string? type)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.LackKVStoreKeyAttributeError, nameof(LackKVStoreKeyAttributeError));

            exception.Data["Type"] = type;

            return exception;
        }

        public static Exception Unkown(string? type, string storeName, object? key, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.UnKown, nameof(Unkown), innerException);

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = storeName;
            exception.Data["Key"] = key;

            return exception;
        }

        public static Exception VersionsKeysNotEqualError()
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.VersionsKeysNotEqualError, nameof(VersionsKeysNotEqualError));

            return exception;
        }

        public static Exception Unkown(string? type, string storeName, object? keys, object? values, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.UnKown, nameof(Unkown), innerException);

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = storeName;
            exception.Data["Keys"] = keys;
            exception.Data["Values"] = values;

            return exception;
        }

        public static Exception CacheLoadedLuaNotFound(string instanceName)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.LoadedLuaNotFound, nameof(CacheLoadedLuaNotFound));

            exception.Data["InstanceName"] = instanceName;

            return exception;
        }

        public static Exception KVStoreRedisConnectionFailed(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.RedisConnectionFailed, nameof(KVStoreRedisConnectionFailed), innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        public static Exception KVStoreRedisTimeout(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.KVStoreRedisTimeout, nameof(KVStoreRedisTimeout), innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        public static Exception Unkown(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.UnKown, nameof(Unkown), innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        public static Exception WriteError(string type, string schemaName, object? keys, ErrorCode errorCode)
        {
            KVStoreException exception = new KVStoreException(errorCode, nameof(WriteError));

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = schemaName;
            exception.Data["Keys"] = keys;

            return exception;
        }

        public static Exception NoSuchKVStoreSchema(string schemaName)
        {
            KVStoreException exception = new KVStoreException(ErrorCodes.NotFound, nameof(NoSuchKVStoreSchema));

            exception.Data["SchemaName"] = schemaName;

            return exception;
        }

        internal static Exception OptionsError(string cause)
        {
            return new KVStoreException(ErrorCodes.OptionsError, cause);
        }
    }
}