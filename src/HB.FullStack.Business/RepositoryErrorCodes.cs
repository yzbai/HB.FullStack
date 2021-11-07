using Microsoft.Extensions.Logging;

using System.Collections;

namespace System
{
    /// <summary>
    /// from 7000 - 7999
    /// </summary>
    internal static class RepositoryErrorCodes
    {
        public static ErrorCode CacheKeyNotSet { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 0, nameof(CacheKeyNotSet), "");
        public static ErrorCode CacheValueNotSet { get;} = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 1, nameof(CacheValueNotSet), "");
        public static ErrorCode UtcTicksNotSet { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 2, nameof(UtcTicksNotSet), "");
        public static ErrorCode CacheGetError { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 3, nameof(CacheGetError), "");
        public static ErrorCode CacheMissed { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 4, nameof(CacheMissed), "");
        public static ErrorCode CacheGetEmpty { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 5, nameof(CacheGetEmpty), "");
        public static ErrorCode CacheLockAcquireFailed { get; } = new ErrorCode(ErrorCodeStartIds.REPOSITORY + 6, nameof(CacheLockAcquireFailed), "");
    }

    internal static class RepositoryExceptions
    {
        internal static Exception UtcTicksNotSet(string resourceType, string cacheKey, object? cacheValue)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.UtcTicksNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;
            exception.Data["CacheValue"] = cacheValue;

            return exception;
        }

        internal static Exception CacheValueNotSet(string resourceType, string cacheKey)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.CacheValueNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;

            return exception;
        }

        internal static Exception CacheKeyNotSet(string? resourceType)
        {
            RepositoryException exception = new RepositoryException(RepositoryErrorCodes.CacheKeyNotSet);

            exception.Data["ResourceType"] = resourceType;

            return exception;

        }
    }

    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, string?, Exception> _logCacheGetError = LoggerMessage.Define<string?, string?>(
            LogLevel.Error,
            RepositoryErrorCodes.CacheGetError.ToEventId(),
            "有可能实体定义发生改变，导致缓存读取出错。读取缓存出错，缓存可能已经被删除，继续读取数据库，DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}");

        public static void LogCacheGetError(this ILogger logger, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception ex)
        {
            _logCacheGetError(logger, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), ex); 
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheMissed = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Information,
            RepositoryErrorCodes.CacheMissed.ToEventId(),
            "缓存 Missed. TypeName={TypeName}, DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}");

        public static void LogCacheMissed(this ILogger logger, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues)
        {
            _logCacheMissed(logger, typeName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), null);
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheGetEmpty = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Information,
            RepositoryErrorCodes.CacheGetEmpty.ToEventId(),
            "缓存查询到空值. TypeName={TypeName}, DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}");

        public static void LogCacheGetEmpty(this ILogger logger, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues)
        {
            _logCacheGetEmpty(logger, typeName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), null);
        }

        private static readonly Action<ILogger, string?, string?, string?, string?, Exception?> _logCacheLockAcquireFailed = LoggerMessage.Define<string?, string?, string?, string?>(
            LogLevel.Information,
            RepositoryErrorCodes.CacheGetEmpty.ToEventId(),
            "缓存锁未能占用， 直接读取数据库. TypeName={TypeName}, DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}, LockStatus={LockStatus}");

        public static void LogCacheLockAcquireFailed(this ILogger logger, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, string? lockStatus)
        {
            _logCacheLockAcquireFailed(logger, typeName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues),lockStatus, null);
        }
    }
}