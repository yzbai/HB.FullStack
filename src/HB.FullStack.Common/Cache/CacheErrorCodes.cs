using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CacheErrorCodes
    {
        public static readonly ErrorCode SlidingTimeBiggerThanMaxAlive = new ErrorCode( nameof(SlidingTimeBiggerThanMaxAlive), "");
        public static readonly ErrorCode EntityNotHaveKeyAttribute = new ErrorCode( nameof(EntityNotHaveKeyAttribute), "");
        public static readonly ErrorCode ConvertError = new ErrorCode( nameof(ConvertError), "");
        public static readonly ErrorCode CacheLoadedLuaNotFound = new ErrorCode( nameof(CacheLoadedLuaNotFound), "");
        public static readonly ErrorCode CacheInstanceNotFound = new ErrorCode( nameof(CacheInstanceNotFound), "");
        public static readonly ErrorCode NoSuchDimensionKey = new ErrorCode( nameof(NoSuchDimensionKey), "");
        public static readonly ErrorCode NotEnabledForEntity = new ErrorCode( nameof(NotEnabledForEntity), "");
        public static readonly ErrorCode Unkown = new ErrorCode( nameof(Unkown), "");
        public static readonly ErrorCode NotACacheEntity = new ErrorCode( nameof(NotACacheEntity), "");
        public static readonly ErrorCode UnkownButDeleted = new ErrorCode( nameof(UnkownButDeleted), "");
        public static readonly ErrorCode GetEntitiesErrorButDeleted = new ErrorCode( nameof(GetEntitiesErrorButDeleted), "");
        public static readonly ErrorCode SetEntitiesError = new ErrorCode( nameof(SetEntitiesError), "");

        public static readonly ErrorCode RemoveEntitiesError = new ErrorCode( nameof(RemoveEntitiesError), "");

        public static readonly ErrorCode ForcedRemoveEntitiesError = new ErrorCode( nameof(ForcedRemoveEntitiesError), "");

        public static readonly ErrorCode GetEntitiesError = new ErrorCode( nameof(GetEntitiesError), "");

        public static readonly ErrorCode CacheInvalidationConcurrencyWithEntities = new ErrorCode( nameof(CacheInvalidationConcurrencyWithEntities), "");
        public static readonly ErrorCode CacheInvalidationConcurrencyWithTimestamp = new ErrorCode( nameof(CacheInvalidationConcurrencyWithTimestamp), "");

        public static readonly ErrorCode CacheUpdateVersionConcurrency = new ErrorCode( nameof(CacheUpdateVersionConcurrency), "");


        public static readonly ErrorCode SetError = new ErrorCode( nameof(SetError), "");

        public static readonly ErrorCode RemoveError = new ErrorCode( nameof(RemoveError), "");
        public static readonly ErrorCode GetError = new ErrorCode( nameof(GetError), "");

        public static readonly ErrorCode CacheUpdateTimestampConcurrency = new ErrorCode( nameof(CacheUpdateTimestampConcurrency), "");

        public static readonly ErrorCode RemoveMultipleError = new ErrorCode( nameof(RemoveMultipleError), "");
    }

    public static class CacheExceptions
    {
        public static CacheException CacheSlidingTimeBiggerThanMaxAlive(string type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SlidingTimeBiggerThanMaxAlive);

            exception.Data["Type"] = type;

            return exception;
        }

        public static CacheException CacheEntityNotHaveKeyAttribute(string? type)
        {
            CacheException exception = new CacheException(CacheErrorCodes.EntityNotHaveKeyAttribute);

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

        public static CacheException GetEntitiesErrorButDeleted(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception innerException)
        {
            CacheException exception = new CacheException(CacheErrorCodes.GetEntitiesErrorButDeleted, innerException);

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

        public static CacheException NotEnabledForEntity(string typeName)
        {
            CacheException exception = new CacheException(CacheErrorCodes.NotEnabledForEntity);

            exception.Data["TypeName"] = typeName;

            return exception;
        }

        public static CacheException SetEntitiesError(string? cacheInstanceName, string? typeName, IEnumerable entities, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SetEntitiesError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["Values"] = SerializeUtil.ToJson(entities);

            return exception;
        }

        public static Exception SetError(string key, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.SetError, ex);

            exception.Data["Key"] = key;
            exception.Data["UtcNowTicks"] = utcTicks.Ticks;
            exception.Data["Option"] = SerializeUtil.ToJson(options);

            return exception;
        }

        public static CacheException RemoveEntitiesError(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, IEnumerable updatedVersions, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveEntitiesError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;
            exception.Data["DimensionKeyValues"] = SerializeUtil.ToJson(dimensionKeyValues);
            exception.Data["UpdatedVersions"] = SerializeUtil.ToJson(updatedVersions);

            return exception;
        }

        public static CacheException ForcedRemoveEntitiesError(string? cacheInstanceName, string? typeName, string? dimensionKeyName, IEnumerable dimensionKeyValues, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.ForcedRemoveEntitiesError, ex);

            exception.Data["CacheInstanceName"] = cacheInstanceName;
            exception.Data["TypeName"] = typeName;
            exception.Data["DimensionKeyName"] = dimensionKeyName;
            exception.Data["DimensionKeyValues"] = SerializeUtil.ToJson(dimensionKeyValues);

            return exception;
        }

        public static Exception RemoveError(string key, UtcNowTicks utcTicks, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveError, ex);

            exception.Data["Key"] = key;
            exception.Data["UtcNowTicks"] = utcTicks.Ticks;

            return exception;
        }

        public static Exception RemoveMultipleError(string collectionKey, IEnumerable<string> keys, UtcNowTicks utcTicks, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveMultipleError, ex);

            exception.Data["CollectionKey"] = collectionKey;
            exception.Data["Keys"] = keys.ToJoinedString(",");
            exception.Data["UtcNowTicks"] = utcTicks.Ticks;

            return exception;
        }

        public static Exception RemoveMultipleError(IEnumerable<string> keys, UtcNowTicks utcTicks, Exception ex)
        {
            CacheException exception = new CacheException(CacheErrorCodes.RemoveMultipleError, ex);

            exception.Data["Keys"] = keys.ToJoinedString(",");
            exception.Data["UtcNowTicks"] = utcTicks.Ticks;

            return exception;
        }
    }
}
