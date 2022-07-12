using HB.FullStack.Common.Cache.CachedCollectionItems;

using Microsoft.Extensions.Logging;

using System.Collections;
using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// from 7000 - 7999
    /// </summary>
    internal static class RepositoryErrorCodes
    {

        public static ErrorCode CacheGetError { get; } = new ErrorCode(nameof(CacheGetError), "");
        public static ErrorCode CacheMissed { get; } = new ErrorCode(nameof(CacheMissed), "");
        public static ErrorCode CacheGetEmpty { get; } = new ErrorCode(nameof(CacheGetEmpty), "");
        public static ErrorCode CacheLockAcquireFailed { get; } = new ErrorCode(nameof(CacheLockAcquireFailed), "");
    }

    internal static class RepositoryExceptions
    {

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
            _logCacheLockAcquireFailed(logger, typeName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), lockStatus, null);
        }
    }
}