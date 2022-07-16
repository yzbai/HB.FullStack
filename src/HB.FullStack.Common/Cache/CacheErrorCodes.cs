using System.Collections;
using System.Collections.Generic;

using HB.FullStack.Common.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace System
{
    public static class CacheErrorCodes
    {
        public static readonly ErrorCode SlidingTimeBiggerThanMaxAlive = new ErrorCode(nameof(SlidingTimeBiggerThanMaxAlive), "");
        public static readonly ErrorCode ModelNotHaveKeyAttribute = new ErrorCode(nameof(ModelNotHaveKeyAttribute), "");
        public static readonly ErrorCode ConvertError = new ErrorCode(nameof(ConvertError), "");
        public static readonly ErrorCode CacheLoadedLuaNotFound = new ErrorCode(nameof(CacheLoadedLuaNotFound), "");
        public static readonly ErrorCode CacheInstanceNotFound = new ErrorCode(nameof(CacheInstanceNotFound), "");
        public static readonly ErrorCode NoSuchDimensionKey = new ErrorCode(nameof(NoSuchDimensionKey), "");
        public static readonly ErrorCode NotEnabledForModel = new ErrorCode(nameof(NotEnabledForModel), "");
        public static readonly ErrorCode Unkown = new ErrorCode(nameof(Unkown), "");
        public static readonly ErrorCode NotACacheModel = new ErrorCode(nameof(NotACacheModel), "");
        public static readonly ErrorCode UnkownButDeleted = new ErrorCode(nameof(UnkownButDeleted), "");
        public static readonly ErrorCode GetModelsErrorButDeleted = new ErrorCode(nameof(GetModelsErrorButDeleted), "");
        public static readonly ErrorCode SetModelsError = new ErrorCode(nameof(SetModelsError), "");

        public static readonly ErrorCode RemoveModelsError = new ErrorCode(nameof(RemoveModelsError), "");

        public static readonly ErrorCode ForcedRemoveModelsError = new ErrorCode(nameof(ForcedRemoveModelsError), "");

        public static readonly ErrorCode GetModelsError = new ErrorCode(nameof(GetModelsError), "");

        public static readonly ErrorCode CacheInvalidationConcurrencyWithModels = new ErrorCode(nameof(CacheInvalidationConcurrencyWithModels), "");
        public static readonly ErrorCode CacheInvalidationConcurrencyWithTimestamp = new ErrorCode(nameof(CacheInvalidationConcurrencyWithTimestamp), "");

        public static readonly ErrorCode CacheUpdateVersionConcurrency = new ErrorCode(nameof(CacheUpdateVersionConcurrency), "");

        public static readonly ErrorCode SetError = new ErrorCode(nameof(SetError), "");

        public static readonly ErrorCode RemoveError = new ErrorCode(nameof(RemoveError), "");
        public static readonly ErrorCode GetError = new ErrorCode(nameof(GetError), "");

        public static readonly ErrorCode CacheUpdateTimestampConcurrency = new ErrorCode(nameof(CacheUpdateTimestampConcurrency), "");

        public static readonly ErrorCode RemoveMultipleError = new ErrorCode(nameof(RemoveMultipleError), "");

        public static ErrorCode CacheCollectionKeyNotSame { get; } = new ErrorCode(nameof(CacheCollectionKeyNotSame), "");
        public static ErrorCode CacheKeyNotSet { get; } = new ErrorCode(nameof(CacheKeyNotSet), "");
        public static ErrorCode CacheValueNotSet { get; } = new ErrorCode(nameof(CacheValueNotSet), "");
        public static ErrorCode CachedItemTimestampNotSet { get; } = new ErrorCode(nameof(CachedItemTimestampNotSet), "");
    }

    public static class CacheExceptions
    {
        internal static Exception CachedItemTimestampNotSet(string resourceType, string cacheKey, object? cacheValue)
        {
            return new CacheException(CacheErrorCodes.CachedItemTimestampNotSet, nameof(CachedItemTimestampNotSet), null, new { ResourceType = resourceType, CacheKey = cacheKey, CacheValue = cacheValue });
        }

        internal static Exception CacheValueNotSet(string resourceType, string cacheKey)
        {
            return new CacheException(CacheErrorCodes.CacheValueNotSet, nameof(CacheValueNotSet), null, new { ResourceType = resourceType, CacheKey = cacheKey });
        }

        internal static Exception CacheKeyNotSet(string? resourceType)
        {
            return new CacheException(CacheErrorCodes.CacheKeyNotSet, nameof(CacheKeyNotSet), null, new { ResourceType = resourceType });
        }

        internal static Exception CacheCollectionKeyNotSame(IEnumerable<ICachedCollectionItem> cachedCollectionItems)
        {
            return new CacheException(CacheErrorCodes.CacheCollectionKeyNotSame, nameof(CacheCollectionKeyNotSame), null, new { CachedCollectionItems = cachedCollectionItems });
        }
        public static CacheException CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            return new CacheException(CacheErrorCodes.SlidingTimeBiggerThanMaxAlive, nameof(CacheSlidingTimeBiggerThanMaxAlive), null, new { Type = type });
        }

        public static CacheException CacheModelNotHaveKeyAttribute(string? type)
        {
            return new CacheException(CacheErrorCodes.ModelNotHaveKeyAttribute, nameof(CacheModelNotHaveKeyAttribute), null, new { Type = type });
        }

        public static CacheException ConvertError(string key, Exception innerException)
        {
            return new CacheException(CacheErrorCodes.ConvertError, nameof(ConvertError), innerException, new { Key = key });
        }

        public static CacheException Unkown(object key, object? value, string? method, Exception innerException)
        {
            return new CacheException(CacheErrorCodes.Unkown, nameof(Unkown), innerException, new { Key = key, Value = value, Method = method });

        }

        public static CacheException GetModelsErrorButDeleted(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception innerException)
        {
            return new CacheException(CacheErrorCodes.GetModelsErrorButDeleted, nameof(GetModelsErrorButDeleted), innerException,
                new { CacheInstanceName = cacheInstanceName, Type = typeName, KeyName = dimensionKeyName, KeyValue = dimensionKeyValues });
        }

        public static CacheException GetError(string key, Exception ex)
        {
            return new CacheException(CacheErrorCodes.GetError, nameof(GetError), ex, new { Key = key });

        }

        public static CacheException CacheLoadedLuaNotFound(string? cacheInstanceName)
        {
            return new CacheException(CacheErrorCodes.CacheLoadedLuaNotFound, nameof(CacheLoadedLuaNotFound), null, new { CacheInstanceName = cacheInstanceName });
        }

        public static CacheException InstanceNotFound(string cacheInstanceName)
        {
            return new CacheException(CacheErrorCodes.CacheInstanceNotFound, nameof(InstanceNotFound), null, new { CacheInstanceName = cacheInstanceName });
        }

        public static CacheException NoSuchDimensionKey(string typeName, string dimensionKeyName)
        {
            return new CacheException(CacheErrorCodes.NoSuchDimensionKey, nameof(NoSuchDimensionKey), null, new { Type = typeName, KeyName = dimensionKeyName });
        }

        public static CacheException NotEnabledForModel(string? typeName)
        {
            return new CacheException(CacheErrorCodes.NotEnabledForModel, nameof(NotEnabledForModel), null, new { Type = typeName });
        }

        public static CacheException SetModelsError(string? cacheInstanceName, string? typeName, IEnumerable models, Exception ex)
        {
            return new CacheException(CacheErrorCodes.SetModelsError, nameof(SetModelsError), ex, new { CacheInstancename = cacheInstanceName, Type = typeName, Values = models });
        }

        public static Exception SetError(string key, long timestamp, DistributedCacheEntryOptions options, Exception ex)
        {
            return new CacheException(CacheErrorCodes.SetError, nameof(SetError), ex, new { Key = key, Options = options, Timestamp = timestamp });
        }

        public static CacheException RemoveModelsError(string? cacheInstanceName, string? typeName, string? keyName, IEnumerable keyValues, Exception ex)
        {
            return new CacheException(CacheErrorCodes.RemoveModelsError, nameof(RemoveModelsError), ex,
                new { CacheInstanceName = cacheInstanceName, Type = typeName, KeyName = keyName, KeyValues = keyValues });
        }

        //TODO: 归类这些ErrorCode，没必要这么多。后期需要对那种情况进行单独处理，那么再单独 独立一个ErrorCode

        public static Exception RemoveError(string key, Exception ex)
        {
            return new CacheException(CacheErrorCodes.RemoveError, nameof(RemoveError), ex, new { Key = key });
        }

        public static Exception RemoveMultipleError(string collectionKey, IEnumerable<string> keys, Exception ex)
        {
            return new CacheException(CacheErrorCodes.RemoveMultipleError, nameof(RemoveMultipleError), ex, new { CollectionKey = collectionKey, Keys = keys.ToJoinedString(",") });
        }

        public static Exception RemoveMultipleError(IEnumerable<string> keys, Exception ex)
        {
            return new CacheException(CacheErrorCodes.RemoveMultipleError, nameof(RemoveMultipleError), ex, new { Keys = keys.ToJoinedString(",") });
        }
    }
}
