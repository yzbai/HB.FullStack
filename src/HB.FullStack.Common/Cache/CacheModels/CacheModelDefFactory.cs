using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace HB.FullStack.Common.Cache.CacheModels
{
    public static class CacheModelDefFactory
    {
        private static readonly ConcurrentDictionary<Type, CacheModelDef> _defDict = new ConcurrentDictionary<Type, CacheModelDef>();

        //private static readonly object _lockObj = new object();

        public static CacheModelDef Get<TModel>()
        {
            return _defDict.GetOrAdd(typeof(TModel), type => CreateModelDef(type));

            //Type modelType = typeof(TModel);

            //if (!_defDict.ContainsKey(modelType))
            //{
            //    lock (_lockObj)
            //    {
            //        if (!_defDict.ContainsKey(modelType))
            //        {
            //            _defDict[modelType] = CreateModelDef(modelType);
            //        }
            //    }
            //}

            //return _defDict[modelType];

        }

        private static CacheModelDef CreateModelDef(Type modelType)
        {
            CacheModelDef def = new()
            {
                Name = modelType.Name
            };

            CacheThisModelAttribute? cacheModelAttribute = modelType.GetCustomAttribute<CacheThisModelAttribute>();

            if (cacheModelAttribute == null)
            {
                def.IsCacheable = false;

                return def;
            }

            def.IsCacheable = true;

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
                    def.Dimensions.Add(propertyInfo);
                }
            }

            if (def.KeyProperty == null)
            {
                throw CacheExceptions.CacheModelNotHaveKeyAttribute(type: modelType.FullName);
            }

            return def;
        }
    }
}
