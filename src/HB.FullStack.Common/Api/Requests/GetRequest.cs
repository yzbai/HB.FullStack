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

        public GetRequest(string? condition) : base(HttpMethodName.Get, condition) { }

        public GetRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethodName.Get, condition) { }

        public GetRequest(ApiAuthType apiAuthType, string? condition) : base(apiAuthType, HttpMethodName.Get, condition) { }

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

    public class GetRequest<T, TParent> : GetRequest<T> where T : ApiResource2 where TParent : ApiResource2
    {
        public GetRequest(Guid parentId, string? condition) : base(condition)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Parents.Add((paretnDef.ResName, parentId.ToString()));
        }

        public GetRequest(string apiKeyName, Guid parentId, string? condition) : base(apiKeyName, condition)
        {
            ApiResourceDef paretnDef = ApiResourceDefFactory.Get<TParent>();

            Parents.Add((paretnDef.ResName, parentId.ToString()));
        }
    }

    public class GetRequest<T, TParent1, TParent2> : GetRequest<T> where T : ApiResource2 where TParent1 : ApiResource2 where TParent2 : ApiResource2
    {
        public GetRequest(Guid parent1Id, Guid parent2Id, string? condition) : base(condition)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Parents.Add((paretn1Def.ResName, parent1Id.ToString()));

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Parents.Add((paretn2Def.ResName, parent2Id.ToString()));
        }

        public GetRequest(string apiKeyName, Guid parent1Id, Guid parent2Id, string? condition) : base(apiKeyName, condition)
        {
            ApiResourceDef paretn1Def = ApiResourceDefFactory.Get<TParent1>();

            Parents.Add((paretn1Def.ResName, parent1Id.ToString()));

            ApiResourceDef paretn2Def = ApiResourceDefFactory.Get<TParent2>();

            Parents.Add((paretn2Def.ResName, parent2Id.ToString()));
        }
    }
}