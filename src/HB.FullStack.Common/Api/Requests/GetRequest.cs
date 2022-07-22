using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// GET /Model
    /// </summary>
    public sealed class GetRequest<T> : ApiRequest where T : ApiResource
    {
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        /// <summary>
        /// 包含哪些子资源
        /// </summary>
        public string? Includes { get; set; }

        public IDictionary<string, string?> PropertyValues { get; } = new Dictionary<string, string?>();

        public GetRequest() : base(typeof(T).Name, ApiMethodName.Get, null, null) { }

        public void OrderBy(params Expression<Func<T, object>>[]? orderBys)
        {
            if (orderBys.IsNullOrEmpty())
            {
                return;
            }

            StringBuilder orderByBuilder = new StringBuilder();

            foreach (Expression<Func<T, object>> orderBy in orderBys)
            {
                string orderByName = ((MemberExpression)orderBy.Body).Member.Name;
                orderByBuilder.Append(orderByName);
                orderByBuilder.Append(',');
            }

            orderByBuilder.RemoveLast();

            OrderBys = orderByBuilder.ToString();
        }

        public GetRequest<T> Include<TRes>() where TRes : ApiResource
        {
            string resName = typeof(TRes).Name;

            if (Includes == null)
            {
                Includes = resName;
            }
            else
            {
                Includes += $",{resName}";
            }

            return this;
        }

        public GetRequest<T> FilterBy(Expression<Func<T, bool>> filterExp)
        {
            //TODO: 实现这个
            throw new NotImplementedException();
        }

        public GetRequest<T> FilterBy(string propertyName, object? propertyValue)
        {
            //TODO: 需要检查PropertyName是否属于Res

            PropertyValues[propertyName] = propertyValue;

            return this;
        }
    }
}