using System;
using System.Collections.Concurrent;
using System.Reflection;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Cache
{
    internal class CacheModelDefFactory : ICacheModelDefFactory, IModelDefProvider
    {
        private readonly ConcurrentDictionary<Type, CacheModelDef?> _defDict = new ConcurrentDictionary<Type, CacheModelDef?>();



        //private static readonly object _lockObj = new object();

        public CacheModelDef? GetDef<T>() => GetDef(typeof(T));


        public CacheModelDef? GetDef(Type type)
        {
            return _defDict.GetOrAdd(type, type => CreateModelDef(type));
        }

        private static CacheModelDef? CreateModelDef(Type modelType)
        {
            CacheModelAttribute? cacheModelAttribute = modelType.GetCustomAttribute<CacheModelAttribute>();

            if (cacheModelAttribute == null)
            {
                return null;
            }

            CacheModelDef def = new()
            {
                Kind = ModelKind.Cache,
                Name = modelType.Name
            };

            def.CacheInstanceName = cacheModelAttribute.CacheInstanceName;

            def.SlidingTime = cacheModelAttribute.SlidingSeconds == -1 ? null : TimeSpan.FromSeconds(cacheModelAttribute.SlidingSeconds);

            def.AbsoluteTimeRelativeToNow = cacheModelAttribute.MaxAliveSeconds == -1 ? null : TimeSpan.FromSeconds(cacheModelAttribute.MaxAliveSeconds);

            if (def.SlidingTime > def.AbsoluteTimeRelativeToNow)
            {
                throw CacheExceptions.CacheSlidingTimeBiggerThanMaxAlive(type: def.Name);
            }

            bool foundkeyAttribute = false;

            foreach (PropertyInfo propertyInfo in modelType.GetProperties())
            {
                if (!foundkeyAttribute)
                {
                    CacheModelKeyAttribute? keyAttribute = propertyInfo.GetCustomAttribute<CacheModelKeyAttribute>();

                    if (keyAttribute != null)
                    {
                        def.KeyProperty = propertyInfo;
                        foundkeyAttribute = true;

                        continue;
                    }
                }

                CacheModelAltKeyAttribute? dimensionKeyAttribute = propertyInfo.GetCustomAttribute<CacheModelAltKeyAttribute>();

                if (dimensionKeyAttribute != null)
                {
                    def.AltKeyProperties.Add(propertyInfo);
                }
            }

            if (def.KeyProperty == null)
            {
                throw CacheExceptions.CacheModelNotHaveKeyAttribute(type: modelType.FullName);
            }

            return def;
        }

        #region IModelDefProvider

        ModelKind IModelDefProvider.ModelKind => ModelKind.Cache;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);

        #endregion
    }
}
