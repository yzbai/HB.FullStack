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

        [JsonIgnore]
        public string? ResourceName { get; set; }

        [JsonIgnore]
        public string? ResourceCollectionName { get; set; }

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

        [JsonIgnore]
        public string RandomStr { get; } = SecurityUtil.CreateRandomString(6);

        [JsonIgnore]
        public long Timestamp { get; } = TimeUtil.UtcNowUnixTimeMilliseconds;

        #endregion

        protected ApiRequest(
            HttpMethod httpMethod, 
            ApiAuthType apiAuthType, 
            string? endPointName, 
            string? apiVersion, 
            string? resourceName, 
            string? resourceCollectionName, 
            string? condition, 
            TimeSpan? rateLimit)
        {
            ApiAuthType = apiAuthType;
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            HttpMethod = httpMethod;
            ResourceName = resourceName;
            ResourceCollectionName = resourceCollectionName;
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
                { ClientNames.RANDOM_STR, RandomStr },
                { ClientNames.TIMESTAMP, Timestamp.ToString(CultureInfo.InvariantCulture)},
                
                //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
                { ClientNames.DEVICE_ID, DeviceId }
            };

            return UriUtil.AddQuerys(uri, parameters);
        }

        protected virtual string GetUrlCore()
        {
            return CreateDefaultUrl();
        }

        /// <summary>
        /// 样式: /[Version]/[ResourceCollection]/[Condition]
        /// </summary>
        protected string CreateDefaultUrl()
        {
            StringBuilder requestUrlBuilder = new();

            if (!ApiVersion.IsNullOrEmpty())
            {
                requestUrlBuilder.Append(ApiVersion);
            }

            if (!ResourceName.IsNullOrEmpty())
            {
                requestUrlBuilder.Append('/');
                requestUrlBuilder.Append(ResourceCollectionName);
            }

            if (!Condition.IsNullOrEmpty())
            {
                requestUrlBuilder.Append('/');
                requestUrlBuilder.Append(Condition);
            }

            return requestUrlBuilder.ToString();
        }

        public abstract string ToDebugInfo();

        public sealed override int GetHashCode()
        {
            HashCode childHashCode = GetChildHashCode();

            childHashCode.Add(DeviceId);
            childHashCode.Add(DeviceVersion);
            childHashCode.Add(PublicResourceToken);
            childHashCode.Add(EndpointName);
            childHashCode.Add(ApiVersion);
            childHashCode.Add(ApiAuthType);
            childHashCode.Add(ApiKeyName);
            childHashCode.Add(Condition);
            childHashCode.Add(ResourceName);
            childHashCode.Add(ResourceCollectionName);

            return childHashCode.ToHashCode();
        }

        protected abstract HashCode GetChildHashCode();
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

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethod httpMethod, string? condition) : base(httpMethod, apiAuthType, null, null, null, null, condition, null)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResourceName = def.ResourceName;
            ResourceCollectionName = def.ResourceCollectionName;
            RateLimit = def.RateLimit;
        }
    }
}