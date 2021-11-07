#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiRequest : ValidatableObject
    {
        /// <summary>
        /// 客户端的DeviceId
        /// </summary>
        public string DeviceId { get; set; } = null!;
        /// <summary>
        /// 客户端的版本
        /// </summary>
        public string DeviceVersion { get; set; } = null!;
        /// <summary>
        /// PRT
        /// </summary>
        public string? PublicResourceToken { get; set; }

        #region Others

        [JsonIgnore]
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        [JsonIgnore]
        public HttpMethod HttpMethod { get; } = null!;

        [JsonIgnore]
        public bool NeedHttpMethodOveride { get; } = true;

        [JsonIgnore]
        public string? EndpointName { get; set; }

        [JsonIgnore]
        public string? ApiVersion { get; set; }

        //[JsonIgnore]
        //public string? ResourceName { get; set; }

        //[JsonIgnore]
        //public string? ResourceCollectionName { get; set; }

        [JsonIgnore]
        public string? ResName { get; set; }

        [JsonIgnore]
        public string? Condition { get; set; }

        [JsonIgnore]
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        [JsonIgnore]
        public ApiAuthType ApiAuthType { get; }

        [JsonIgnore]
        public string? ApiKeyName { get; set; }

        /// <summary>
        /// 请求间隔
        /// </summary>
        [JsonIgnore]
        public TimeSpan? RateLimit { get; set; }

        //[JsonIgnore]
        //public string RandomStr { get; } = SecurityUtil.CreateRandomString(6);

        #endregion

        protected ApiRequest(
            HttpMethod httpMethod,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition,
            TimeSpan? rateLimit)
        {
            ApiAuthType = apiAuthType;
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            HttpMethod = httpMethod;
            ResName = resName;
            Condition = condition;
            RateLimit = rateLimit;
        }

        public void SetJwt(string jwt)
        {
            Headers["Authorization"] = "Bearer " + jwt;
        }

        public void SetApiKey(string apiKey)
        {
            Headers["X-Api-Key"] = apiKey;
        }

        public string GetUrl()
        {
            string uri = GetUrlCore();

            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)},
                
                //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
                { ClientNames.DEVICE_ID, DeviceId }
            };

            return UriUtil.AddQuerys(uri, parameters);
        }

        protected virtual string GetUrlCore()
        {
            return $"{ApiVersion}/{ResName}/{Condition}";
        }

        public abstract string ToDebugInfo();

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(DeviceId);
            hashCode.Add(DeviceVersion);
            hashCode.Add(PublicResourceToken);
            hashCode.Add(EndpointName);
            hashCode.Add(ApiVersion);
            hashCode.Add(ApiAuthType);
            hashCode.Add(ApiKeyName);
            hashCode.Add(Condition);
            hashCode.Add(ResName);

            return hashCode.ToHashCode();
        }
    }

    public abstract class ApiRequest<T> : ApiRequest where T : ApiResource2
    {
        /// <summary>
        /// 因为不会直接使用ApiRequest作为Api的请求参数，所以不用提供无参构造函数，而具体的子类需要提供
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="condition">同一Verb下的条件分支，比如在ApiController上标注的[HttpGet("BySms")],BySms就是condition</param>
        protected ApiRequest(HttpMethod httpMethod, string? condition) : this(ApiAuthType.Jwt, httpMethod, condition)
        {
        }

        protected ApiRequest(string apiKeyName, HttpMethod httpMethod, string? condition) : this(ApiAuthType.ApiKey, httpMethod, condition)
        {
            ApiKeyName = apiKeyName;
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethod httpMethod, string? condition) : base(httpMethod, apiAuthType, null, null, null, condition, null)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;
            RateLimit = def.RateLimit;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public abstract class ApiRequest<T, TSub> : ApiRequest where T : ApiResource2 where TSub : ApiResource2
    {
        /// <summary>
        /// 主要Resource 的ID
        /// </summary>
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public string SubResName { get; set; } = null!;

        protected ApiRequest(Guid id, HttpMethod httpMethod, string? condition) : this(id, ApiAuthType.Jwt, httpMethod, condition)
        {
        }

        protected ApiRequest(Guid id, string apiKeyName, HttpMethod httpMethod, string? condition) : this(id, ApiAuthType.ApiKey, httpMethod, condition)
        {
            ApiKeyName = apiKeyName;
        }

        protected ApiRequest(Guid id, ApiAuthType apiAuthType, HttpMethod httpMethod, string? condition) : base(httpMethod, apiAuthType, null, null, null, condition, null)
        {
            Id = id;

            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;
            RateLimit = def.RateLimit;

            ApiResourceDef subDef = ApiResourceDefFactory.Get<TSub>();

            SubResName = subDef.ResName;
        }

        protected override string GetUrlCore()
        {
            return $"{ApiVersion}/{ResName}/{Id}/{SubResName}/{Condition}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SubResName, Id);
        }
    }
}