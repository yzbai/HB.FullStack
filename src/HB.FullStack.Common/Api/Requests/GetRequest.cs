using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public abstract class GetRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [JsonIgnore]
        public int? Page { get; set; }

        [JsonIgnore]
        public int? PerPage { get; set; }

        [JsonIgnore]
        public string? OrderBy { get; set; }

        protected GetRequest(string? condition) : base(HttpMethod.Get, condition) { }

        protected GetRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethod.Get, condition) { }    

        protected sealed override string GetUrlCore()
        {
            string url = CreateUrl();

            return AddCommonQueryToUrl(url);
        }

        protected virtual string CreateUrl()
        {
            return CreateDefaultUrl();
        }

        protected sealed override HashCode GetChildHashCode()
        {
            HashCode code = GetHashCodeCore();

            code.Add(Page);
            code.Add(PerPage);
            code.Add(OrderBy);

            return code;
        }

        protected abstract HashCode GetHashCodeCore();

        private string AddCommonQueryToUrl(string url)
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
    }
}
