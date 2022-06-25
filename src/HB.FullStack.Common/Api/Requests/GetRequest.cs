using System;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetRequest<T> : ApiRequest where T : ApiResource2
    {
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string? OrderBys { get; set; }

        [OnlyForJsonConstructor]
        public GetRequest() { }

        public GetRequest(HttpRequestMessageBuilder httpRequestBuilder) : base(httpRequestBuilder) { }

        /// <summary>
        ///指定的AuthType可以覆盖ApiResourceDef中定义的
        /// </summary>
        public GetRequest(string? condition, ApiAuthType? authType = null) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Get, condition))
        {
            if(authType != null)
            {
                RequestBuilder!.AuthType = authType.Value;
            }
        }

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