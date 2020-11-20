using System;
using System.Collections.Generic;
using System.Reflection;
using HB.Framework.Common.Cache;
using HB.Framework.Common.Entities;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class CacheEntityDefFactory
    {
        private static readonly Dictionary<Type, CacheEntityDef> _defDict = new Dictionary<Type, CacheEntityDef>();

        private static object _lockObj = new object();

        public static CacheEntityDef Get<TEntity>() where TEntity : Entity, new()
        {
            Type entityType = typeof(TEntity);

            if (!_defDict.ContainsKey(entityType))
            {
                lock (_lockObj)
                {
                    if (!_defDict.ContainsKey(entityType))
                    {
                        _defDict[entityType] = CreateEntityDef(entityType);
                    }
                }
            }

            return _defDict[entityType];

        }

        private static CacheEntityDef CreateEntityDef(Type entityType)
        {
            CacheEntityDef def = new CacheEntityDef
            {
                Name = entityType.Name
            };

            CacheEntityAttribute? cacheEntityAttribute = entityType.GetCustomAttribute<CacheEntityAttribute>();

            if (cacheEntityAttribute == null)
            {
                def.IsCacheable = false;

                return def;
            }

            def.CacheInstanceName = cacheEntityAttribute.CacheInstanceName;

            def.EntryOptions = new DistributedCacheEntryOptions();

            def.EntryOptions.SlidingExpiration = cacheEntityAttribute.SlidingAliveTime ?? CacheOptions.SlidingAliveTime;

            if (cacheEntityAttribute.MaxAliveTime != null)
            {
                def.EntryOptions.AbsoluteExpiration = DateTimeOffset.UtcNow + cacheEntityAttribute.MaxAliveTime;
            }

            bool foundGuidKeyAttribute = false;

            entityType.GetProperties().ForEach(propertyInfo =>
            {

                if (!foundGuidKeyAttribute)
                {
                    CacheGuidKeyAttribute? guidKeyAttribute = propertyInfo.GetCustomAttribute<CacheGuidKeyAttribute>();

                    if (guidKeyAttribute != null)
                    {
                        def.GuidKeyProperty = propertyInfo;
                        foundGuidKeyAttribute = true;

                        return;
                    }
                }

                CacheDifferentDimensionKeyAttribute? dimensionKeyAttribute = propertyInfo.GetCustomAttribute<CacheDifferentDimensionKeyAttribute>();

                if (dimensionKeyAttribute != null)
                {
                    def.OtherDimensions.Add(propertyInfo);
                }
            });

            if (def.GuidKeyProperty == null)
            {
                throw new CacheException(ErrorCode.CacheEntityNotHaveGuidKeyAttribute, $"entity:{entityType.FullName}");
            }

            return def;
        }
    }
}
