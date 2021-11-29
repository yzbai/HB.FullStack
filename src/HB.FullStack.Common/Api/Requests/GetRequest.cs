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

        public GetRequest(string? condition = null) : base(HttpMethodName.Get, condition) { }

        public GetRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethodName.Get, condition) { }

        public GetRequest(ApiAuthType apiAuthType, string? condition) : base(apiAuthType, HttpMethodName.Get, condition) { }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{ResName}/{Condition}";

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Page, PerPage, OrderBys);
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

    public class GetRequest<T, TSub> : ApiRequest<T, TSub> where T : ApiResource2 where TSub : ApiResource2
    {
        #region Querys

        [JsonIgnore]
        public int? Page { get; set; }

        [JsonIgnore]
        public int? PerPage { get; set; }

        [JsonIgnore]
        public string? OrderBy { get; set; }

        #endregion

        public GetRequest(Guid id, string? condition = null) : this(id, ApiAuthType.Jwt, condition)
        {
        }

        public GetRequest(Guid id, string apiKeyName, string? condition) : this(id, ApiAuthType.ApiKey, condition)
        {
            ApiKeyName = apiKeyName;
        }

        public GetRequest(Guid id, ApiAuthType apiAuthType, string? condition) : base(id, apiAuthType, HttpMethodName.Get, condition)
        {
        }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{ResName}/{Id}/{SubResName}/{Condition}";

            return AddCommonQueryToUrl(url);
        }

        protected string AddCommonQueryToUrl(string url)
        {
            Dictionary<string, string?> parameters = new Dictionary<string, string?>();

            if (Page.HasValue && PerPage.HasValue)
            {
                parameters.Add(ClientNames.Page, Page.Value.ToString(CultureInfo.InvariantCulture));
                parameters.Add(ClientNames.PerPage, PerPage.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (OrderBy.IsNotNullOrEmpty())
            {
                parameters.Add(ClientNames.OrderBy, OrderBy);
            }

            return parameters.Any() ? UriUtil.AddQuerys(url, parameters) : url;
        }

        public override string ToDebugInfo()
        {
            return $"{GetType().FullName}. Resource:{typeof(T).FullName}, Json:{SerializeUtil.ToJson(this)}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Page, PerPage, OrderBy);
        }
    }
}