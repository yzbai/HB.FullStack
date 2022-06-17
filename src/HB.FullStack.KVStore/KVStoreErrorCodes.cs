using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

[assembly: InternalsVisibleTo("HB.Infrastructure.Redis.KVStore")]
namespace HB.FullStack.KVStore
{
    /// <summary>
    /// from 4000 - 4999
    /// </summary>
    internal static class KVStoreErrorCodes
    {
        public static ErrorCode LoadedLuaNotFound { get; set; } = new ErrorCode(nameof(LoadedLuaNotFound), "");
        public static ErrorCode RedisConnectionFailed { get; set; } = new ErrorCode(nameof(RedisConnectionFailed), "");
        public static ErrorCode KVStoreRedisTimeout { get; set; } = new ErrorCode(nameof(KVStoreRedisTimeout), "");
        public static ErrorCode KVStoreError { get; set; } = new ErrorCode(nameof(KVStoreError), "");
        public static ErrorCode NoSuchInstance { get; set; } = new ErrorCode(nameof(NoSuchInstance), "");
        public static ErrorCode KVStoreExistAlready { get; set; } = new ErrorCode(nameof(KVStoreExistAlready), "");
        public static ErrorCode KVStoreVersionNotMatched { get; set; } = new ErrorCode(nameof(KVStoreVersionNotMatched), "");
        public static ErrorCode NoEntitySchemaFound { get; set; } = new ErrorCode(nameof(NoEntitySchemaFound), "");
        public static ErrorCode LackKVStoreKeyAttributeError { get; set; } = new ErrorCode(nameof(LackKVStoreKeyAttributeError), "");
        public static ErrorCode VersionsKeysNotEqualError { get; set; } = new ErrorCode(nameof(VersionsKeysNotEqualError), "");
        public static ErrorCode UnKown { get; set; } = new ErrorCode(nameof(UnKown), "");
    }

    internal static class Exceptions
    {
        internal static Exception NoEntitySchemaFound(string? type)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.NoEntitySchemaFound);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception LackKVStoreKeyAttributeError(string? type)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.LackKVStoreKeyAttributeError);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception Unkown(string? type, string storeName, object? key, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.UnKown, innerException);

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = storeName;
            exception.Data["Key"] = key;

            return exception;
        }

        internal static Exception VersionsKeysNotEqualError()
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.VersionsKeysNotEqualError);

            return exception;
        }

        internal static Exception Unkown(string? type, string storeName, object? keys, object? values, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.UnKown, innerException);

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = storeName;
            exception.Data["Keys"] = keys;
            exception.Data["Values"] = values;

            return exception;
        }

        internal static Exception CacheLoadedLuaNotFound(string instanceName)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.LoadedLuaNotFound);

            exception.Data["InstanceName"] = instanceName;

            return exception;
        }

        internal static Exception KVStoreRedisConnectionFailed(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.RedisConnectionFailed, innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception KVStoreRedisTimeout(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.KVStoreRedisTimeout, innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception Unkown(string type, Exception innerException)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.UnKown, innerException);

            exception.Data["Type"] = type;

            return exception;
        }

        internal static Exception WriteError(string type, string storeName, object? keys, object? values, ErrorCode errorCode)
        {
            KVStoreException exception = new KVStoreException(errorCode);

            exception.Data["Type"] = type;
            exception.Data["StoreName"] = storeName;
            exception.Data["Keys"] = keys;
            exception.Data["Values"] = values;

            return exception;
        }

        internal static Exception NoSuchInstance(string instanceName)
        {
            KVStoreException exception = new KVStoreException(KVStoreErrorCodes.NoSuchInstance);

            exception.Data["InstanceName"] = instanceName;

            return exception;
        }
    }
}