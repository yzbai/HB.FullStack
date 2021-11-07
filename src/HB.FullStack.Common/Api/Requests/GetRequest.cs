using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public string? OrderBy { get; set; }

        #endregion

        protected GetRequest(string? condition) : base(HttpMethod.Get, condition) { }

        protected GetRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethod.Get, condition) { }

        protected GetRequest(ApiAuthType apiAuthType, string? condition) : base(apiAuthType, HttpMethod.Get, condition) { }

        protected override string GetUrlCore()
        {
            string url = $"{ApiVersion}/{ResName}/{Condition}";

            return AddCommonQueryToUrl(url);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Page, PerPage, OrderBy);
        }

        protected string AddCommonQueryToUrl(string url)
        {
            Dictionary<string, string?> parameters = new Dictionary<string, string?>();

            if (Page.HasValue && PerPage.HasValue)
            {
                parameters.Add(ClientNames.PAGE, Page.Value.ToString(CultureInfo.InvariantCulture));
                parameters.Add(ClientNames.PER_PAGE, PerPage.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (OrderBy.IsNotNullOrEmpty())
            {
                parameters.Add(ClientNames.ORDER_BY, OrderBy);
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

        public GetRequest(Guid id, string? condition) : this(id, ApiAuthType.Jwt, condition)
        {
        }

        public GetRequest(Guid id, string apiKeyName, string? condition) : this(id, ApiAuthType.ApiKey, condition)
        {
            ApiKeyName = apiKeyName;
        }

        public GetRequest(Guid id, ApiAuthType apiAuthType, string? condition) : base(id, apiAuthType, HttpMethod.Get, condition)
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
                parameters.Add(ClientNames.PAGE, Page.Value.ToString(CultureInfo.InvariantCulture));
                parameters.Add(ClientNames.PER_PAGE, PerPage.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (OrderBy.IsNotNullOrEmpty())
            {
                parameters.Add(ClientNames.ORDER_BY, OrderBy);
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
