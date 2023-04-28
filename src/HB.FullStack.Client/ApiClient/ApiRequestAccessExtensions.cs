using System;
using System.Collections.Generic;
using System.Reflection;
using HB.FullStack.Common.Meta;

namespace HB.FullStack.Client.ApiClient
{
    public static class ApiRequestAccessExtensions
    {
        #region RequestQuery

        public static string? BuildQueryString(this object? obj)
        {
            if (obj == null)
            {
                return null;
            }

            Func<object, List<string>> func = GetCachedConvertRequestToQueriesFunc(obj.GetType());

            List<string> queries = func(obj);

            return queries.ToJoinedString("&");
        }

        private static readonly Dictionary<Type, Func<object, List<string>>> _convertRequestToQueriesDict = new Dictionary<Type, Func<object, List<string>>>();

        //TODO: do we need a lock?
        private static Func<object, List<string>> GetCachedConvertRequestToQueriesFunc(Type type)
        {
            if (_convertRequestToQueriesDict.TryGetValue(type, out Func<object, List<string>>? cachedFunc))
            {
                return cachedFunc;
            }

            IList<PropertyInfo> requestQueryProperties = ReflectionUtil.GetPropertyInfosByAttribute<RequestQueryAttribute>(type);

            Func<object, List<string>> func = MetaAccess.CreateConvertPropertiesToQueriesDelegate(type, requestQueryProperties);

            _convertRequestToQueriesDict.TryAdd(type, func);

            return func;
        }

        #endregion

        #region RequestBody

        public static object? GetRequestBody(this object? obj)
        {
            if (obj == null)
            {
                return null;
            }

            Func<object, object?> func = GetCachedPropertyGetFunc(obj.GetType());

            return func(obj);
        }

        private static readonly Dictionary<Type, Func<object, object?>> _requestBodyGetDict = new Dictionary<Type, Func<object, object?>>();

        private static Func<object, object?> GetCachedPropertyGetFunc(Type type)
        {
            if (_requestBodyGetDict.TryGetValue(type, out Func<object, object?>? cachedFunc))
            {
                return cachedFunc;
            }

            PropertyInfo? requestBodyProperty = ReflectionUtil.GetPropertyInfoByAttribute<RequestBodyAttribute>(type);

            Func<object, object?> func = MetaAccess.CreatePropertyGetDelegateByIL(requestBodyProperty);

            _requestBodyGetDict.TryAdd(type, func);

            return func;
        }

        #endregion
    }
}
