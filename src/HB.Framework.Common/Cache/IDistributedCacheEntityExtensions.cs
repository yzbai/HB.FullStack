using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheEntityExtensions
    {
        /// <summary>
        /// 返回值：（cached，isExist）
        /// </summary>
        public static async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(this IDistributedCache cache, string cacheKeyName, string cacheKeyValue)
            where TEntity : Entity, new()
        {
            if (!cache.IsEnabled<TEntity>())
            {
                return (null, false);
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string? guidKeyValue = cacheKeyValue;

            if (!cacheKeyName.Equals(entityDef.GuidKeyProperty.Name, GlobalSettings.ComparisonIgnoreCase))
            {
                //其他维度
                string key1 = GetDimensionKey(entityDef.Name, cacheKeyName, cacheKeyValue);

                guidKeyValue = await cache.GetStringAsync(key1).ConfigureAwait(false);

                if (string.IsNullOrEmpty(guidKeyValue))
                {
                    return (null, false);
                }
            }

            string key2 = GetGuidKey(entityDef.Name, entityDef.GuidKeyProperty.Name, guidKeyValue);

            return await cache.GetAsync<TEntity>(key2).ConfigureAwait(false);
        }

        private static string GetGuidKey(string entityName, string guidKeyPropertyName, string guidKeyValue)
        {
            return $"{entityName}_{guidKeyPropertyName}_{guidKeyValue}";
        }

        private static string GetDimensionKey(string entityName, string dimensionKeyName, string dimensionKeyValue)
        {
            return $"{entityName}_{dimensionKeyName}_{dimensionKeyValue}";
        }

        public static Task SetEntityAsync<TEntity>(this IDistributedCache cache, TEntity entity) where TEntity : Entity, new()
        {
            if (!cache.IsEnabled<TEntity>())
            {
                return Task.CompletedTask;
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            List<Task> tasks = new List<Task>();

            //Guid 
            string guidValue = (string)entityDef.GuidKeyProperty.GetValue(entity);
            string guidKey = GetGuidKey(entityDef.Name, entityDef.GuidKeyProperty.Name, guidValue);

            tasks.Add(cache.SetAsync<TEntity>(guidKey, entity, entityDef.EntryOptions));

            //OtherDimension
            entityDef.OtherDimensions.ForEach(propertyInfo =>
            {
                string dimensionKey = GetDimensionKey(entityDef.Name, propertyInfo.Name, (string)propertyInfo.GetValue(entity));
                tasks.Add(cache.SetStringAsync(dimensionKey, guidValue));
            });

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 可以存储空值
        /// </summary>
        public static async Task SetEntityAsync<TEntity>(this IDistributedCache cache, string cacheKeyName, string cacheKeyValue, TEntity? entity)
            where TEntity : Entity, new()
        {
            if (!cache.IsEnabled<TEntity>())
            {
                return;
            }

            if (entity != null)
            {
                await cache.SetEntityAsync(entity).ConfigureAwait(false);
                return;
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string key = GetDimensionKey(entityDef.Name, cacheKeyName, cacheKeyValue);

            await cache.SetStringAsync(key, null, entityDef.EntryOptions).ConfigureAwait(false);
        }

        public static Task RemoveEntityAsync<TEntity>(this IDistributedCache cache, TEntity entity) where TEntity : Entity, new()
        {
            if (!cache.IsEnabled<TEntity>())
            {
                return Task.CompletedTask;
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            List<string> keysToRemove = new List<string>();

            string guidKeyValue = (string)entityDef.GuidKeyProperty.GetValue(entity);
            keysToRemove.Add(GetGuidKey(entityDef.Name, entityDef.GuidKeyProperty.Name, guidKeyValue));

            foreach (PropertyInfo propertyInfo in entityDef.OtherDimensions)
            {
                keysToRemove.Add(GetDimensionKey(entityDef.Name, propertyInfo.Name, (string)propertyInfo.GetValue(entityDef)));
            }

            List<Task> tasks = new List<Task>();

            foreach (string key in keysToRemove)
            {
                tasks.Add(cache.RemoveAsync(key));
            }

            return Task.WhenAll(tasks);
        }

        public static bool IsEnabled<TEntity>(this IDistributedCache cache) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsCacheable;
        }

        /// <summary>
        /// 是否允许进行多key同时查询
        /// //TODO: 暂不支持
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static bool IsMultipleEnabled<TEntity>(this IDistributedCache cache)
        {
            return false;
        }
    }
}
