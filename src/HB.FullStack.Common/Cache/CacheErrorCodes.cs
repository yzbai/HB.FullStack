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
            CacheException exception = new CacheException(CacheErrorCodes.CachedItemTimestampNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;
            exception.Data["CacheValue"] = cacheValue;

            return exception;
        }

        internal static Exception CacheValueNotSet(string resourceType, string cacheKey)
        {
            CacheException exception = new CacheException(CacheErrorCodes.CacheValueNotSet);

            exception.Data["ResourceType"] = resourceType;
            exception.Data["CacheKey"] = cacheKey;

            return exception;
        }

        internal static Exception CacheKeyNotSet(string? resourceType)
        {
            CacheException exception = new CacheException(CacheErrorCodes.CacheKeyNotSet);

            exception.Data["ResourceType"] = resourceType;

            return exception;

        }

        internal static Exception CacheCollectionKeyNotSame(IEnumerable<ICachedCollectionItem> cachedCollectionItems)
        {
            CacheException exception = new CacheException(CacheErrorCodes.CacheCollectionKeyNotSame);
            exception.Data["CachedCollectionItems"] = SerializeUtil.ToJson(cachedCollectionItems);

            return exception;
        }
        public static CacheException CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SlidingTimeBiggerThanMaxAlive);

            exception.Data["Type"] = type;

            return exception;
        }

        public static CacheException CacheModelNotHaveKeyAttribute(string? type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ModelNotHaveKeyAttribute);

            exception.Data["Type"] = type;

            return exception;
        }

        public static CacheException ConvertError(string key, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ConvertError, innerException);

            exception.Data["Key"] = key;

            return exception;
        }

        public static CacheException Unkown(object key, object? value, string? method, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.Unkown, innerException);

            exception.Data["Key"] = key;
            exception.Data["Value"] = value;
            exception.Data["Method"] = method;

            return exception;
        }

        public static CacheException GetModelsErrorButDeleted(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.GetModelsErrorButDeleted, innerException);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;
            exception.Data["DimensionKeyValues"] = SerializeUtil.ToJson(dimensionKeyValues);

            return exception;
        }

        public static CacheException GetError(string key, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.GetError, ex);

            exception.Data["Key"] = key;

            return exception;
        }

        public static CacheException CacheLoadedLuaNotFound(string? cacheInstanceName)
        {
            CacheException exception = new CacheException(CacheErrorCodes.CacheLoadedLuaNotFound);

            exception.Data["CacheInstanceName"] = cacheInstanceName;

            return exception;
        }

        public static CacheException InstanceNotFound(string cacheInstanceName)
        {
            CacheException exception = new CacheException(CacheErrorCodes.CacheInstanceNotFound);

            exception.Data["CacheInstanceName"] = cacheInstanceName;

            return exception;
        }

        public static CacheException NoSuchDimensionKey(string typeName, string dimensionKeyName)
        {
            CacheException exception = new CacheException(CacheErrorCodes.NoSuchDimensionKey);

            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;

            return exception;
        }

        public static CacheException NotEnabledForModel(string? typeName)
        {
            CacheException exception = new CacheException(CacheErrorCodes.NotEnabledForModel);

            exception.Data["TypeName"] = typeName;

            return exception;
        }

        public static CacheException SetModelsError(string? cacheInstanceName, string? typeName, IEnumerable models, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SetModelsError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["Values"] = SerializeUtil.ToJson(models);

            return exception;
        }

        public static Exception SetError(string key, long timestamp, DistributedCacheEntryOptions options, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SetError, ex);

            exception.Data["Key"] = key;
            exception.Data["Option"] = SerializeUtil.ToJson(options);
            exception.Data["Timestamp"] = timestamp;

            return exception;
        }

        public static CacheException RemoveModelsError(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveModelsError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;
            exception.Data["DimensionKeyValues"] = SerializeUtil.ToJson(dimensionKeyValues);

            return exception;
        }

        public static CacheException ForcedRemoveModelsError(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ForcedRemoveModelsError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;
            exception.Data["DimensionKeyValues"] = SerializeUtil.ToJson(dimensionKeyValues);

            return exception;
        }

        public static Exception RemoveError(string key, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveError, ex);

            exception.Data["Key"] = key;

            return exception;
        }

        public static Exception RemoveMultipleError(string collectionKey, IEnumerable<string> keys, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveMultipleError, ex);

            exception.Data["CollectionKey"] = collectionKey;
            exception.Data["Keys"] = keys.ToJoinedString(",");

            return exception;
        }

        public static Exception RemoveMultipleError(IEnumerable<string> keys, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveMultipleError, ex);

            exception.Data["Keys"] = keys.ToJoinedString(",");

            return exception;
        }
    }
}
