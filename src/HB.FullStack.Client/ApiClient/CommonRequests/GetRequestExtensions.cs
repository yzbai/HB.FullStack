using System;
using System.Linq.Expressions;

using HB.FullStack.Common.Convert;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    public static class GetRequestExtensions
    {
        public static GetRequest Id(this GetRequest request, object id)
        {
            request.Ids.Add(id.ToString()!);

            return request;
        }

        public static GetRequest Page(this GetRequest request, int page, int perPage)
        {
            request.Page = page;
            request.PerPage = perPage;

            return request;
        }

        public static GetRequest Include<TRes>(this GetRequest request) where TRes : SharedResource
        {
            return request.Include(typeof(TRes).Name);
        }

        public static GetRequest Include(this GetRequest request, string resName)
        {
            request.ResIncludes.Add(resName);

            return request;
        }

        public static GetRequest OrderBy<T>(this GetRequest request, Expression<Func<T, object>> orderByExp) where T : SharedResource
        {

            string orderByPropertyName = ((MemberExpression)orderByExp.Body).Member.Name;

            request.OrderBys = request.OrderBys.Append(orderByPropertyName, ',');

            return request;
        }

        public static GetRequest Where<T>(this GetRequest request, Expression<Func<T, bool>> filterExp)
        {
            //TODO: 实现这个
            throw new NotImplementedException();
        }

        public static GetRequest Where(this GetRequest request, string propertyName, object? propertyValue)
        {
            //TODO: 考虑提高性能
            //是否建立统一的ApiResourceDefFactory?

            //var resDef = ApiResourceDefFactory.GetDef(request.ResName);

            //if (resDef?.GetPropertyDef(propertyName) == null)
            //{
            //    throw CommonExceptions.ApiResourceError("不存在这样的属性", null, new { ResName = request.ResName, PropertyName = propertyName });
            //}

            string? propertyValueString = StringConvertCenter.ConvertToString(propertyValue, null, StringConvertPurpose.HTTP_QUERY);

            request.WherePropertyNames.Add(propertyName);
            request.WherePropertyValues.Add(propertyValueString);

            return request;
        }

        public static GetRequest Auth(this GetRequest request, ApiRequestAuth auth)
        {
            request.Auth = auth;
            return request;
        }
    }
}