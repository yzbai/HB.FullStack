using HB.FullStack.Common;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logLuaScriptNotLoaded = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.CacheLoadedLuaNotFound.ToEventId(),
            "Redis没有加载LuaScript。将要加载并重试。CacheInstance={CacheInstance}, EntityName={EntityName}, Method={Method}");

        public static void LogLuaScriptNotLoaded(this ILogger logger, string? cacheInstance, string? entityName, string? method)
        {
            _logLuaScriptNotLoaded(logger, cacheInstance, entityName, method, null);
        }

        private static readonly Action<ILogger, string?, string?, string?, string?, Exception?> _logGetEntitiesError = LoggerMessage.Define<string?, string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.GetEntitiesError.ToEventId(),
            "分析这个GetEntitiesAsync.情况1，程序中实体改了. CacheInstance={CacheInstance}, EntityName={EntityName}, DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}");

        public static void LogGetEntitiesError(this ILogger logger, string? cacheInstance, string? entityName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception? ex)
        {
            _logGetEntitiesError(logger, cacheInstance, entityName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), ex);
        }

        private static readonly Action<ILogger, string?, string?, string?, string?, Exception?> _logForcedRemoveError = LoggerMessage.Define<string?, string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.ForcedRemoveEntitiesError.ToEventId(),
            "在强制删除中出错. CacheInstance={CacheInstance}, EntityName={EntityName}, DimensionKeyName={DimensionKeyName}, DimensionKeyValues={DimensionKeyValues}");

        public static void LogForcedRemoveError(this ILogger logger, string? cacheInstance, string? entityName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception? ex)
        {
            _logForcedRemoveError(logger, cacheInstance, entityName, dimensionKeyName, SerializeUtil.ToJson(dimensionKeyValues), ex);
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheInvalidationConcurrencyWithEntities = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.CacheInvalidationConcurrencyWithEntities.ToEventId(),
            "检测到，Cache Invalidation Concurrency冲突，已被阻止. CacheInstance={CacheInstance}, EntityName={EntityName}, Object={Object}");

        public static void LogCacheInvalidationConcurrencyWithEntities(this ILogger logger, string? cacheInstance, string? entityName, object? obj)
        {
            _logCacheInvalidationConcurrencyWithEntities(logger, cacheInstance, entityName, SerializeUtil.ToJson(obj), null);
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheUpdateVersionConcurrency = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.CacheUpdateVersionConcurrency.ToEventId(),
            "检测到，Cache Update Concurrency冲突，已被阻止. CacheInstance={CacheInstance}, EntityName={EntityName}, Object={Object}");

        public static void LogCacheUpdateVersionConcurrency(this ILogger logger, string? cacheInstance, string? entityName, object? obj)
        {
            _logCacheUpdateVersionConcurrency(logger, cacheInstance, entityName, SerializeUtil.ToJson(obj), null);
        }

        private static readonly Action<ILogger, string?, Exception?> _logCacheGetError = LoggerMessage.Define<string?>(
            LogLevel.Error,
            CacheErrorCodes.GetError.ToEventId(),
            "缓存读取错误。Key={Key}");

        public static void LogCacheGetError(this ILogger logger, string? key, Exception? innerException)
        {
            _logCacheGetError(logger, key, innerException);
        }

        private static readonly Action<ILogger, string?, string?, Exception?> _logCacheCollectionGetError = LoggerMessage.Define<string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.GetError.ToEventId(),
            "缓存读取错误。CollectionKey={CollectionKey}, ItemKey={ItemKey}");

        public static void LogCacheCollectionGetError(this ILogger logger, string? collectionKey,string? itemKey, Exception? innerException)
        {
            _logCacheCollectionGetError(logger, collectionKey, itemKey, innerException);
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheInvalidationConcurrencyWithTimestamp = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.CacheInvalidationConcurrencyWithTimestamp.ToEventId(),
            "检测到，Cache Invalidation Concurrency冲突，已被阻止. Key={Key}, UtcNowTicks={UtcNowTicks}, Options={Options}");

        public static void LogCacheInvalidationConcurrencyWithTimestamp(this ILogger logger, string? key, UtcNowTicks utcNowTicks, DistributedCacheEntryOptions options)
        {
            _logCacheInvalidationConcurrencyWithTimestamp(logger, key, utcNowTicks.Ticks.ToString(CultureInfo.InvariantCulture), SerializeUtil.ToJson(options), null);
        }

        private static readonly Action<ILogger, string?, string?, string?, Exception?> _logCacheUpdateTimestampConcurrency = LoggerMessage.Define<string?, string?, string?>(
            LogLevel.Error,
            CacheErrorCodes.CacheUpdateTimestampConcurrency.ToEventId(),
            "检测到，Cache Update Concurrency冲突，已被阻止. Key={Key}, UtcNowTicks={UtcNowTicks}, Options={Options}");
        public static void LogCacheUpdateTimestampConcurrency(this ILogger logger, string? key, UtcNowTicks utcNowTicks, DistributedCacheEntryOptions options)
        {
            _logCacheUpdateTimestampConcurrency(logger, key, utcNowTicks.Ticks.ToString(CultureInfo.InvariantCulture), SerializeUtil.ToJson(options), null);
        }
    }
}
