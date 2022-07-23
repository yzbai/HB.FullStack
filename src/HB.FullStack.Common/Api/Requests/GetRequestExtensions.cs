using System;
using System.Linq.Expressions;

namespace HB.FullStack.Common.Api
{
    public static class GetRequestExtensions
    {
        public static GetRequest Include<TRes>(this GetRequest request) where TRes : ApiResource
        {
            string resName = typeof(TRes).Name;

            request.Includes = request.Includes.Append(resName, ',');

            return request;
        }

        public static GetRequest OrderBy<T>(this GetRequest request, Expression<Func<T, object>> orderByExp) where T : ApiResource
        {

            string orderByPropertyName = ((MemberExpression)orderByExp.Body).Member.Name;

            request.OrderBys = request.OrderBys.Append(orderByPropertyName, ',');

            return request;
        }

        public static GetRequest FilterBy<T>(this GetRequest request, Expression<Func<T, bool>> filterExp)
        {
            //TODO: 实现这个
            throw new NotImplementedException();
        }

        public static GetRequest FilterBy(this GetRequest request, string propertyName, object? propertyValue, PropertyFilterOperator @operator)
        {
            //TODO: 考虑提高性能
            //是否建立统一的ApiResourceDefFactory?

            var resDef = ApiResourceDefFactory.GetDef(request.ResName);

            if (resDef?.GetPropertyDef(propertyName) == null)
            {
                throw ApiExceptions.ApiResourceError("不存在这样的属性", null, new { ResName = request.ResName, PropertyName = propertyName });
            }

            request.PropertyFilters.Add(new PropertyFilter
            {
                PropertyName = propertyName,
                Operator = @operator,
                PropertyStringValue = TypeStringConverter.ConvertToString(propertyValue)
            });

            return request;
        }

        public static GetRequest Auth(this GetRequest request, ApiRequestAuth auth)
        {
            request.Auth = auth;
            return request;
        }
    }
}