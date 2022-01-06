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
    /// <summary>
    /// 强调构造条件
    /// Url
    /// HttpMethod
    /// Headers
    /// </summary>
    public class ApiRequestBuilder
    {
        public HttpMethodName HttpMethod { get; }

        public ApiAuthType ApiAuthType { get; }

        public bool NeedHttpMethodOveride { get; }

        public string? EndpointName { get; set; }

        public string? ApiVersion { get; set; }

        public string? Condition { get; set; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public string? ApiKeyName { get; set; }

        public string? ResName { get; set; }

        public Guid? ResId { get; set; }

        public IList<(string parentResName, string parentResId)> Parents { get; } = new List<(string parentResName, string parentResId)>();

        public void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = "Bearer " + jwt;
        }

        public void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public void SetDeviceId(string deviceId)
        {
            Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public string GetUrl(string deviceId)
        {
            string uri = GetUrlCore();

            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)},

                //额外添加DeviceId，为了验证jwt中的DeviceId与本次请求deviceiId一致
                //已经移动到Header中去
                //{ ClientNames.DEVICE_ID, deviceId }
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

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(EndpointName);
            hashCode.Add(NeedHttpMethodOveride);
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

        public ApiRequestBuilder(HttpMethodName httpMethod,
            bool needHttpMethodOveride,
            ApiAuthType apiAuthType,
            string? endPointName,
            string? apiVersion,
            string? resName,
            string? condition)
        {
            HttpMethod = httpMethod;
            NeedHttpMethodOveride = needHttpMethodOveride;
            ApiAuthType = apiAuthType;
            EndpointName = endPointName;
            ApiVersion = apiVersion;
            ResName = resName;
            Condition = condition;
        }
    }

    /// <summary>
    /// 只强调数据
    /// </summary>
    public abstract class ApiRequest : ValidatableObject
    {
        [JsonIgnore]
        public ApiRequestBuilder? Builder { get; }

        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        public string DeviceId { get; set; } = null!;

        public string DeviceVersion { get; set; } = null!;

        protected ApiRequest(HttpMethodName httpMethod, ApiAuthType apiAuthType, string? endPointName, string? apiVersion, string? resName, string? condition)
        {
            Builder = new ApiRequestBuilder(httpMethod, true, apiAuthType, endPointName, apiVersion, resName, condition);
        }

        protected ApiRequest(ApiRequestBuilder apiRequestBuilder)
        {
            Builder = apiRequestBuilder;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            if (Builder != null)
            {
                hashCode.Add(Builder.GetHashCode());
            }

            hashCode.Add(DeviceId);
            hashCode.Add(DeviceVersion);

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
            Builder!.ApiKeyName = apiKeyName;
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethodName httpMethod, string? condition)
            : base(httpMethod, apiAuthType, null, null, null, condition)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            Builder!.EndpointName = def.EndpointName;
            Builder.ApiVersion = def.ApiVersion;
            Builder.ResName = def.ResName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}