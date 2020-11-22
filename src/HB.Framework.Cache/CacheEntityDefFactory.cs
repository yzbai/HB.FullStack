using System;
using System.Collections.Generic;
using System.Reflection;
using HB.Framework.Common.Entities;

namespace HB.Framework.Cache
{
    public static class CacheEntityDefFactory
    {
        private static readonly Dictionary<Type, CacheEntityDef> _defDict = new Dictionary<Type, CacheEntityDef>();

        private static readonly object _lockObj = new object();

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

            def.IsCacheable = true;

            def.IsBatchEnabled = cacheEntityAttribute.IsBatchEnabled;

            def.CacheInstanceName = cacheEntityAttribute.CacheInstanceName;

            def.SlidingTime = cacheEntityAttribute.SlidingSeconds == -1 ? null : (TimeSpan?)TimeSpan.FromSeconds(cacheEntityAttribute.SlidingSeconds);

            def.AbsoluteTimeRelativeToNow = cacheEntityAttribute.MaxAliveSeconds == -1 ? null : (TimeSpan?)TimeSpan.FromSeconds(cacheEntityAttribute.MaxAliveSeconds);

            if (def.SlidingTime > def.AbsoluteTimeRelativeToNow)
            {
                throw new CacheException(ErrorCode.CacheSlidingTimeBiggerThanMaxAlive, $"{def.Name}");
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
                    def.Dimensions.Add(propertyInfo);
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
