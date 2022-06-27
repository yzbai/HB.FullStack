using System;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        [OnlyForJsonConstructor]
        public GetRequest() { }

        public GetRequest(ApiRequestAuth auth, string? condition) : base(ApiMethodName.Get, auth, condition) { }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(Page);
            hashCode.Add(PerPage);
            hashCode.Add(OrderBys);

            return hashCode.ToHashCode();
        }

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
    }
}