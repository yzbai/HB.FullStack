﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace HB.FullStack.Common.Api
{
    //TODO: 除了从ApiResourceAttribute里获得配置外，增加Configuration读取
    public static class ApiResourceDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ApiResourceDef> _defDict = new ConcurrentDictionary<Type, ApiResourceDef>();

        public static ApiResourceDef Get<T>() where T : ApiResource2
        {
            return _defDict.GetOrAdd(typeof(T), t => CreateResourceDef(t));
        }

        private static ApiResourceDef CreateResourceDef(Type type)
        {
            var attr = type.GetCustomAttribute<ApiResourceAttribute>();

            if (attr == null)
            {
                throw ApiExceptions.LackApiResourceAttribute(type: type.FullName);
            }

            return new ApiResourceDef
            {
                ApiVersion = attr.Version,
                EndpointName = attr.EndPointName,
                ResName = attr.ResName ?? type.Name,
                OwnerResName = attr.OwnerResName
            };
        }
    }

}