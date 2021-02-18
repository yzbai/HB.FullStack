#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public enum ApiAuthType
    {
        None,
        Jwt,
        ApiKey
    }

#pragma warning disable CA1024 // Use properties where appropriate //All use fields & Get Methods instead of Properties, for avoid mvc binding & json searilize
    public abstract class ApiRequest : ValidatableObject
    {
        #region Common Parameters

        public string DeviceId { get; set; } = null!;

        public DeviceInfos DeviceInfos { get; set; } = null!;

        public string DeviceVersion { get; set; } = null!;

        public string? PublicResourceToken { get; set; }

        #endregion

        private readonly string _requestId = SecurityUtil.CreateUniqueToken();
        private bool _needHttpMethodOveride = true;
        private string _endpointName = null!;
        private string _apiVersion = null!;
        private string _resourceName = null!;

        private HttpMethod _httpMethod = null!;
        private string? _condition;
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();

        private ApiAuthType _apiAuthType;
        private string? _apiKeyName;

        private TimeSpan? _rateLimit;

        protected ApiRequest() { }

        protected ApiRequest(ApiAuthType apiAuthType, string endPointName, string apiVersion, HttpMethod httpMethod, string resourceName, string? condition)
        {
            _apiAuthType = apiAuthType;
            _endpointName = endPointName;
            _apiVersion = apiVersion;
            _httpMethod = httpMethod;
            _resourceName = resourceName;
            _condition = condition;
        }

        public ApiAuthType GetApiAuthType() => _apiAuthType;

        public void SetApiAuthType(ApiAuthType apiAuthType)
        {
            _apiAuthType = apiAuthType;
        }

        public string GetRequestId()
        {
            return _requestId;
        }

        public string GetEndpointName()
        {
            return _endpointName;
        }

        public void SetEndpointName(string endpointName)
        {
            _endpointName = endpointName;
        }

        public string GetApiVersion()
        {
            return _apiVersion;
        }

        public void SetApiVersion(string apiVersion)
        {
            _apiVersion = apiVersion;
        }

        public HttpMethod GetHttpMethod()
        {
            return _httpMethod;
        }

        public void SetHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
        }

        public string GetResourceName()
        {
            return _resourceName;
        }

        public void SetResourceName(string resourceName)
        {
            _resourceName = resourceName;
        }

        public string? GetCondition()
        {
            return _condition;
        }

        public void SetCondition(string? conditon)
        {
            _condition = conditon;
        }

        public bool GetNeedHttpMethodOveride()
        {
            return _needHttpMethodOveride;
        }

        public void SetNeedHttpMethodOveride(bool isNeeded)
        {
            _needHttpMethodOveride = isNeeded;
        }

        public static string GetRandomStr()
        {
            return SecurityUtil.CreateRandomString(6);
        }

        public TimeSpan? GetRateLimit()
        {
            return _rateLimit;
        }

        public void SetRateLimit(TimeSpan timeSpan)
        {
            _rateLimit = timeSpan;
        }

        public string? GetHeader(string name)
        {
            if (_headers.TryGetValue(name, out string? value))
            {
                return value;
            }

            return null;
        }

        public void SetHeader(string name, string value)
        {
            _headers[name] = value;
        }

        public IDictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        public void SetJwt(string jwt)
        {
            SetHeader("Authorization", "Bearer " + jwt);
        }

        public void SetApiKey(string apiKey)
        {
            SetHeader("X-Api-Key", apiKey);
        }

        public void SetApiKeyName(string apiKeyName)
        {
            _apiKeyName = apiKeyName;
        }

        public string? GetApiKeyName()
        {
            return _apiKeyName;
        }

        public string GetLastUser()
        {
            return DeviceInfos.Name;
        }
    }
#pragma warning restore CA1024 // Use properties where appropriate

    public abstract class ApiRequest<T> : ApiRequest where T : ApiResource
    {
        /// <summary>
        /// 因为不会直接使用ApiRequest作为Api的请求参数，所以不用提供无参构造函数，而具体的子类需要提供
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="condition">同一Verb下的条件分支，比如在ApiController上标注的[HttpGet("BySms")],BySms就是condition</param>
        protected ApiRequest(HttpMethod httpMethod, string? condition)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            SetEndpointName(def.EndpointName);
            SetApiVersion(def.ApiVersion);
            SetResourceName(def.Name);
            SetHttpMethod(httpMethod);
            SetCondition(condition);
            SetApiAuthType(ApiAuthType.Jwt);
        }

        protected ApiRequest(string apiKeyName, HttpMethod httpMethod, string? condition)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            SetEndpointName(def.EndpointName);
            SetApiVersion(def.ApiVersion);
            SetResourceName(def.Name);
            SetHttpMethod(httpMethod);
            SetCondition(condition);
            SetApiAuthType(ApiAuthType.ApiKey);
            SetApiKeyName(apiKeyName);
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethod httpMethod, string? condition)
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<T>();

            SetEndpointName(def.EndpointName);
            SetApiVersion(def.ApiVersion);
            SetResourceName(def.Name);
            SetHttpMethod(httpMethod);
            SetCondition(condition);
            SetApiAuthType(apiAuthType);
        }

        protected ApiRequest(ApiAuthType apiAuthType, HttpMethod httpMethod, string? condition, string endpointName, string apiVersion, string resourceName)
        {
            SetEndpointName(endpointName);
            SetApiVersion(apiVersion);
            SetResourceName(resourceName);
            SetHttpMethod(httpMethod);
            SetCondition(condition);
            SetApiAuthType(apiAuthType);
        }

        public abstract override int GetHashCode();
    }
}