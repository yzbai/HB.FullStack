﻿
using System;
using System.Collections.Concurrent;
using System.Reflection;
using HB.FullStack.Common.Api.Resources;

namespace HB.FullStack.Common.Api
{
    public static class ApiResourceBindingFactory
    {
        private static readonly ConcurrentDictionary<Type, ApiResourceBinding?> _defDict = new ConcurrentDictionary<Type, ApiResourceBinding?>();

        public static ApiResourceBinding? Get<T>() where T : ApiResource
        {
            return _defDict.GetOrAdd(typeof(T), t => CreateResourceDef(t));
        }

        private static ApiResourceBinding? CreateResourceDef(Type type)
        {
            //TODO: 除了从ApiResourceAttribute里获得配置外，增加Configuration读取.并且Configuration可以覆盖Attribute设置

            var attr = type.GetCustomAttribute<ApiResourceBindingAttribute>();

            if (attr == null)
            {
                return null;
            }

            ApiResourceBinding def = new ApiResourceBinding
            {
                EndpointName = attr.EndPointName,
                Version = attr.Version,
                ControllerModelName = attr.ControllerModelName,
                //Parent1ModelName = attr.Parent1ModelName,
                //Parent2ModelName = attr.Parent2ModelName,
            };

            //if (def.Parent1ModelName.IsNotNullOrEmpty())
            //{
            //    MethodInfo? getter = type.GetGetterMethodByAttribute<Parent1ModelIdAttribute>();

            //    if (getter == null)
            //    {
            //        throw ApiExceptions.LackParent1ResAttribute(type);
            //    }

            //    def.Parent1ResIdGetMethod = getter;
            //}

            //if (def.Parent2ModelName.IsNotNullOrEmpty())
            //{
            //    MethodInfo? getter = type.GetGetterMethodByAttribute<Parent2ModelIdAttribute>();

            //    if (getter == null)
            //    {
            //        throw ApiExceptions.LackParent2ResAttribute(type);
            //    }

            //    def.Parent2ResIdGetMethod = getter;
            //}

            return def;
        }

        public static void Register<T>(ApiResourceBinding def) where T : ApiResource
        {
            _ = _defDict.AddOrUpdate(typeof(T), def, (_, _) => def);
        }
    }

}