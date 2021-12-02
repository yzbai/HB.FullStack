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
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        /// <summary>
        /// 客户端的DeviceId
        /// </summary>
        public string DeviceId { get; set; } = null!;
        /// <summary>
        /// 客户端的版本
        /// </summary>
        public string DeviceVersion { get; set; } = null!;

        #region Others 在服务器端不可用，因为是JsonIgnore

        [JsonIgnore]
        public HttpMethodName HttpMethod { get; }

        [JsonIgnore]
        public bool NeedHttpMethodOveride { get; } = true;

        [JsonIgnore]
        public string? EndpointName { get; set; }

        [JsonIgnore]
        public string? ApiVersion { get; set; }

        [JsonIgnore]
        public string? Condition { get; set; }

        [JsonIgnore]
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        [JsonIgnore]
        public ApiAuthType ApiAuthType { get; }

        [JsonIgnore]
        public string? ApiKeyName { get; set; }

        [JsonIgnore]
        public string? ResName { get; set; }

        [JsonIgnore]
        public Guid? ResId { get; set; }

        [JsonIgnore]
        public IList<(string parentResName, string parentResId)> Parents { get; } = new List<(string parentResName, string parentResId)>();

        #endregion

        protected ApiRequest(
            HttpMethodName httpMethod,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition)
        {
            ApiAuthType = apiAuthType;
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            HttpMethod = httpMethod;
            ResName = resName;
            Condition = condition;
        }

        public void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = "Bearer " + jwt;
        }

        public void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
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
            string? parentSegment = GetParentSegment(Parents);

            if (parentSegment == null && ResId == null)
            {
                return $"{ApiVersion}/{ResName}/{Condition}";
            }
            else if (parentSegment == null && ResId != null)
            {
                return $"{ApiVersion}/{ResName}/{ResId}/{Condition}";
            }
            else if (parentSegment != null && ResId == null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{Condition}";
            }
            else //if(parentSegment != null && ResId != null)
            {
                return $"{ApiVersion}/{parentSegment}/{ResName}/{ResId}/{Condition}";
            }

            static string? GetParentSegment(IList<(string parentResName, string parentResId)> lst)
            {
                if (lst.IsNullOrEmpty())
                {
                    return null;
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var (parentResName, parentResId) in lst)
                {
                    stringBuilder.Append(parentResName);
                    stringBuilder.Append('/');
                    stringBuilder.Append(parentResId);
                    stringBuilder.Append('/');
                }

                return stringBuilder.RemoveLast().ToString();
            }
        }

        public abstract string ToDebugInfo();

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(DeviceId);
            hashCode.Add(DeviceVersion);
            hashCode.Add(EndpointName);
            hashCode.Add(ApiVersion);
            hashCode.Add(ApiAuthType);
            hashCode.Add(ApiKeyName);
            hashCode.Add(Condition);
            hashCode.Add(ResName);
            hashCode.Add(ResId);
            hashCode.Add(HttpMethod);

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }
            foreach ((string parentResName, string parentResId) in Parents)

            {
                hashCode.Add(parentResId);
                hashCode.Add(parentResName);
            }

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
        protected ApiRequest(HttpMethodName httpMethod, string? condition) : this(ApiAuthType.Jwt, httpMethod, condition)
        {
        }

        protected ApiRequest(string apiKeyName, HttpMethodName httpMethod, string? condition) : this(ApiAuthType.ApiKey, httpMethod, condition)
        {
            ApiKeyName = apiKeyName;
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethodName httpMethod, string? condition)
            : base(httpMethod, apiAuthType, null, null, null, condition)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            EndpointName = def.EndpointName;
            ApiVersion = def.ApiVersion;
            ResName = def.ResName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}