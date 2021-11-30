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

    public class GetRequest2<T, TOwner> : GetRequest<T> where T : ApiResource2 where TOwner : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// 服务器端不可用
        /// </summary>
        [JsonIgnore]
        public string OwnerResName { get; set; } = null!;

        public GetRequest2(Guid ownerId, string? condition = null) : base(condition)
        {
            ApiResourceDef ownerDef = ApiResourceDefFactory.Get<TOwner>();
            OwnerId = ownerId;
            OwnerResName = ownerDef.ResName;
        }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{OwnerResName}/{OwnerId}/{ResName}/{Condition}";

            return AddCommonQueryToUrl(url);
        }

        public override string ToDebugInfo()
        {
            return $"{GetType().FullName}. Resource:{typeof(T).FullName}, Json:{SerializeUtil.ToJson(this)}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), OwnerId, OwnerResName);
        }
    }
}