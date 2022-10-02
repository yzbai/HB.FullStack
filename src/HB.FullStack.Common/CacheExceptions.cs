using System.Collections;
using System.Collections.Generic;

using HB.FullStack.Common.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace System
{
    public static class CacheExceptions
    {
        internal static Exception CachedItemTimestampNotSet(string resourceType, string cacheKey, object? cacheValue)
        {
            return new CacheException(ErrorCodes.CachedItemTimestampNotSet, nameof(CachedItemTimestampNotSet), null, new { ResourceType = resourceType, CacheKey = cacheKey, CacheValue = cacheValue });
        }

        internal static Exception CacheValueNotSet(string resourceType, string cacheKey)
        {
            return new CacheException(ErrorCodes.CacheValueNotSet, nameof(CacheValueNotSet), null, new { ResourceType = resourceType, CacheKey = cacheKey });
        }

        internal static Exception CacheKeyNotSet(string? resourceType)
        {
            return new CacheException(ErrorCodes.CacheKeyNotSet, nameof(CacheKeyNotSet), null, new { ResourceType = resourceType });
        }

        internal static Exception CacheCollectionKeyNotSame(IEnumerable<ICachedCollectionItem> cachedCollectionItems)
        {
            return new CacheException(ErrorCodes.CacheCollectionKeyNotSame, nameof(CacheCollectionKeyNotSame), null, new { CachedCollectionItems = cachedCollectionItems });
        }
        public static CacheException CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            return new CacheException(ErrorCodes.SlidingTimeBiggerThanMaxAlive, nameof(CacheSlidingTimeBiggerThanMaxAlive), null, new { Type = type });
        }

        public static CacheException CacheModelNotHaveKeyAttribute(string? type)
        {
            return new CacheException(ErrorCodes.ModelNotHaveKeyAttribute, nameof(CacheModelNotHaveKeyAttribute), null, new { Type = type });
        }

        public static CacheException ConvertError(string key, Exception innerException)
        {
            return new CacheException(ErrorCodes.ConvertError, nameof(ConvertError), innerException, new { Key = key });
        }

        public static CacheException Unkown(object key, object? value, string? method, Exception innerException)
        {
            return new CacheException(ErrorCodes.UnKown, nameof(Unkown), innerException, new { Key = key, Value = value, Method = method });

        }

        public static CacheException GetModelsErrorButDeleted(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception innerException)
        {
            return new CacheException(ErrorCodes.GetModelsErrorButDeleted, nameof(GetModelsErrorButDeleted), innerException,
                new { CacheInstanceName = cacheInstanceName, Type = typeName, KeyName = dimensionKeyName, KeyValue = dimensionKeyValues });
        }

        public static CacheException GetError(string key, Exception ex)
        {
            return new CacheException(ErrorCodes.GetError, nameof(GetError), ex, new { Key = key });

        }

        public static CacheException CacheLoadedLuaNotFound(string? cacheInstanceName)
        {
            return new CacheException(ErrorCodes.CacheLoadedLuaNotFound, nameof(CacheLoadedLuaNotFound), null, new { CacheInstanceName = cacheInstanceName });
        }

        public static CacheException InstanceNotFound(string cacheInstanceName)
        {
            return new CacheException(ErrorCodes.CacheInstanceNotFound, nameof(InstanceNotFound), null, new { CacheInstanceName = cacheInstanceName });
        }

        public static CacheException NoSuchDimensionKey(string typeName, string dimensionKeyName)
        {
            return new CacheException(ErrorCodes.NoSuchDimensionKey, nameof(NoSuchDimensionKey), null, new { Type = typeName, KeyName = dimensionKeyName });
        }

        public static CacheException NotEnabledForModel(string? typeName)
        {
            return new CacheException(ErrorCodes.NotEnabledForModel, nameof(NotEnabledForModel), null, new { Type = typeName });
        }

        public static CacheException SetModelsError(string? cacheInstanceName, string? typeName, IEnumerable models, Exception ex)
        {
            return new CacheException(ErrorCodes.SetModelsError, nameof(SetModelsError), ex, new { CacheInstancename = cacheInstanceName, Type = typeName, Values = models });
        }

        public static Exception SetError(string key, long timestamp, DistributedCacheEntryOptions options, Exception ex)
        {
            return new CacheException(ErrorCodes.SetError, nameof(SetError), ex, new { Key = key, Options = options, Timestamp = timestamp });
        }

        public static CacheException RemoveModelsError(string? cacheInstanceName, string? typeName, string? keyName, IEnumerable keyValues, Exception ex)
        {
            return new CacheException(ErrorCodes.RemoveModelsError, nameof(RemoveModelsError), ex,
                new { CacheInstanceName = cacheInstanceName, Type = typeName, KeyName = keyName, KeyValues = keyValues });
        }

        //TODO: 归类这些ErrorCode，没必要这么多。后期需要对那种情况进行单独处理，那么再单独 独立一个ErrorCode

        public static Exception RemoveError(string key, Exception ex)
        {
            return new CacheException(ErrorCodes.RemoveError, nameof(RemoveError), ex, new { Key = key });
        }

        public static Exception RemoveMultipleError(string collectionKey, IEnumerable<string> keys, Exception ex)
        {
            return new CacheException(ErrorCodes.RemoveMultipleError, nameof(RemoveMultipleError), ex, new { CollectionKey = collectionKey, Keys = keys.ToJoinedString(",") });
        }

        public static Exception RemoveMultipleError(IEnumerable<string> keys, Exception ex)
        {
            return new CacheException(ErrorCodes.RemoveMultipleError, nameof(RemoveMultipleError), ex, new { Keys = keys.ToJoinedString(",") });
        }
    }
}
