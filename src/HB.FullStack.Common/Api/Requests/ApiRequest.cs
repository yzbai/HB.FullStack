

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public abstract class ApiRequestBuilder
    {
        public abstract void SetDeviceId(string deviceId);
        public abstract void SetDeviceVersion(string deviceVersion);
        public abstract void SetJwt(string jwt);
        public abstract void SetApiKey(string apiKey);

        public abstract void AddParent(string parentResName, string parentResId);

        public string GetUrl()
        {
            string uri = GetUrlCore();

            IDictionary<string, string?> parameters = new Dictionary<string, string?>
            {
                { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)}
            };

            return UriUtil.AddQuerys(uri, parameters);
        }

        protected abstract string GetUrlCore();
    }

    /// <summary>
    /// 强调构造条件
    /// Url
    /// HttpMethod
    /// Headers
    /// </summary>
    public class DefaultApiRequestBuilder : ApiRequestBuilder
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

        public override void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = "Bearer " + jwt;
        }

        public override void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public override void SetDeviceId(string deviceId)
        {
            Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public override void SetDeviceVersion(string deviceVersion)
        {
            Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

        public override void AddParent(string parentResName, string parentResId)
        {
            Parents.Add((parentResName, parentResId));
        }

        protected override string GetUrlCore()
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

        public DefaultApiRequestBuilder(HttpMethodName httpMethod,
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

    public class PlainUrlRequestBuilder : ApiRequestBuilder
    {
        public override void SetDeviceId(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override void SetDeviceVersion(string deviceVersion)
        {
            throw new NotImplementedException();
        }

        public override void SetJwt(string jwt)
        {
            throw new NotImplementedException();
        }

        public override void SetApiKey(string apiKey)
        {
            throw new NotImplementedException();
        }

        protected override string GetUrlCore()
        {
            throw new NotImplementedException();
        }

        public override void AddParent(string parentResName, string parentResId)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 只强调数据
    /// </summary>
    public abstract class ApiRequest : ValidatableObject
    {
        /// <summary>
        /// 为了客户端的Request建立，不包含具体请求数据
        /// </summary>
        [JsonIgnore]
        public ApiRequestBuilder? Builder { get; }

        /// <summary>
        /// TODO: 防止同一个RequestID两次被处理
        /// </summary>
        public string RequestId { get; } = SecurityUtil.CreateUniqueToken();

        /// <summary>
        /// 为了反序列化
        /// </summary>
        protected ApiRequest()
        { }

        protected ApiRequest(HttpMethodName httpMethod, ApiAuthType apiAuthType, string? endPointName, string? apiVersion, string? resName, string? condition)
        {
            Builder = new DefaultApiRequestBuilder(httpMethod, true, apiAuthType, endPointName, apiVersion, resName, condition);
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

            return hashCode.ToHashCode();
        }
    }

    public abstract class ApiRequest<T> : ApiRequest where T : ApiResource2
    {
        /// <summary>
        /// 为了反序列化
        /// </summary>
        protected ApiRequest()
        { }

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
            if (Builder is DefaultApiRequestBuilder builder)
            {
                builder.ApiKeyName = apiKeyName;
            }
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethodName httpMethod, string? condition)
            : base(httpMethod, apiAuthType, null, null, null, condition)
        {
            if (Builder is DefaultApiRequestBuilder builder)
            {
                ApiResourceDef def = ApiResourceDefFactory.Get<T>();

                builder.EndpointName = def.EndpointName;
                builder.ApiVersion = def.ApiVersion;
                builder.ResName = def.ResName;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}