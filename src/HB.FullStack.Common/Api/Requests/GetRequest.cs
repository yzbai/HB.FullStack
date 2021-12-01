using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        #region Querys

        [JsonIgnore]
        public int? Page { get; set; }

        [JsonIgnore]
        public int? PerPage { get; set; }

        [JsonIgnore]
        public IEnumerable<Expression<Func<T, object>>> OrderBys { get; } = new List<Expression<Func<T, object>>>();

        #endregion

        public GetRequest(string? condition, Guid? ownerResId, Guid? resId) : base(HttpMethodName.Get, condition,ownerResId, resId) { }

        public GetRequest(string apiKeyName, string? condition, Guid? ownerResId, Guid? resId) : base(apiKeyName, HttpMethodName.Get, condition, ownerResId, resId) { }

        protected override string GetUrlCore()
        {
            string url = base.GetUrlCore();

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            
            hashCode.Add(Page);
            hashCode.Add(PerPage);

            foreach (var exp in OrderBys)
            {
                hashCode.Add(exp.GetHashCode());
            }

            return hashCode.ToHashCode();
        }

        protected string AddCommonQueryToUrl(string url)
        {
            Dictionary<string, string?> parameters = new Dictionary<string, string?>();

            if (Page.HasValue && PerPage.HasValue)
            {
                parameters.Add(ClientNames.Page, Page.Value.ToString(CultureInfo.InvariantCulture));
                parameters.Add(ClientNames.PerPage, PerPage.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (OrderBys.IsNotNullOrEmpty())
            {
                StringBuilder orderByBuilder = new StringBuilder();

                foreach (Expression<Func<T, object>> orderBy in OrderBys)
                {
                    string orderByName = ((MemberExpression)orderBy.Body).Member.Name;
                    orderByBuilder.Append(orderByName);
                    orderByBuilder.Append(',');
                }

                orderByBuilder.RemoveLast();

                parameters.Add(ClientNames.OrderBy, orderByBuilder.ToString());
            }

            return parameters.Any() ? UriUtil.AddQuerys(url, parameters) : url;
        }

        public override string ToDebugInfo()
        {
            return $"{GetType().FullName}. Resource:{typeof(T).FullName}, Json:{SerializeUtil.ToJson(this)}";
        }
    }
}