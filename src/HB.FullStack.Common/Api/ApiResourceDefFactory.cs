using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace HB.FullStack.Common.Api
{
    public static class ApiResourceDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ApiResourceDef> _defDict = new ConcurrentDictionary<Type, ApiResourceDef>();

        //private static readonly object _defDictLocker = new object();

        /// <summary>
        /// Get
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public static ApiResourceDef Get<T>() where T : ApiResource2
        {
            return _defDict.GetOrAdd(typeof(T), t => CreateResourceDef(t));

            //Type type = typeof(T);

            //if (_defDict.TryGetValue(type, out ApiResourceDef? def))
            //{
            //    return def;
            //}

            //lock (_defDictLocker)
            //{
            //    if (_defDict.TryGetValue(type, out def))
            //    {
            //        return def;
            //    }

            //    def = CreateResourceDef(type);

            //    _defDict[type] = def;

            //    return def;
            //}
        }

        /// <summary>
        /// CreateResourceDef
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private static ApiResourceDef CreateResourceDef(Type type)
        {
            var attr = type.GetCustomAttribute<ApiResourceAttribute>();

            if (attr == null)
            {
                throw ApiExceptions.NotApiResourceEntity(type: type.FullName);
            }

            return new ApiResourceDef
            {
                RateLimit = attr.RateLimit,
                ApiVersion = attr.Version,
                EndpointName = attr.EndPointName,
                Name = type.Name
            };
        }
    }
}
