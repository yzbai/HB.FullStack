using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public static class ApiRequestExtensions
    {
        #region RequestQuery

        public static string? BuildQueryString(this ApiRequest request)
        {
            Func<object, List<string>> func = GetCachedConvertRequestToQueriesFunc(request.GetType());

            List<string> queries = func(request);

            return queries.ToJoinedString("&");
        }

        private static readonly Dictionary<Type, Func<object, List<string>>> _convertRequestToQueriesDict = new Dictionary<Type, Func<object, List<string>>>();

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

        public static object? GetRequestBody(this ApiRequest request)
        {
            Func<object, object?> func = GetCachedPropertyGetFunc(request.GetType());

            return func(request);
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
