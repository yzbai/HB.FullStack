
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace HB.FullStack.Common.Api
{
    public static class ApiResourceDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ApiResourceDef?> _defDict = new ConcurrentDictionary<Type, ApiResourceDef?>();

        public static ApiResourceDef? Get<T>() where T : ApiResource2
        {
            return _defDict.GetOrAdd(typeof(T), t => CreateResourceDef(t));
        }

        private static ApiResourceDef? CreateResourceDef(Type type)
        {
            //TODO: 除了从ApiResourceAttribute里获得配置外，增加Configuration读取.并且Configuration可以覆盖Attribute设置
            var attr = type.GetCustomAttribute<ApiResourceAttribute>();

            return attr == null ? null : new ApiResourceDef
            {
                EndpointName = attr.EndPointName,
                Version = attr.Version,
                AuthType = attr.AuthType,
                ResName = attr.ResName,
                Parent1ResName = attr.Parent1ResName,
                ApiKeyName = attr.ApiKeyName
            };
        }

        public static void Register<T>(ApiResourceDef def) where T : ApiResource2
        {
            _ = _defDict.AddOrUpdate(typeof(T), def, (_, _) => def);
        }
    }

}