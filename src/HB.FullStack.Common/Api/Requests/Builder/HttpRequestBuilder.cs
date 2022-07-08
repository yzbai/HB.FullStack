using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;
using HB.FullStack.Common.Api.Requests;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 构建一个完整的HttpRequest,需要的信息来自三方面：
    /// 1. Resource定义，即ApiResourceBinding
    /// 2. Request本身，即业务数据和参数
    /// 3. Endpoint Setting，即Server与Client的服务设定
    /// </summary>
    public abstract class HttpRequestBuilder
    {
        #region 由Endpoint决定

        public HttpEndpointSettings EndpointSettings { get; } = new HttpEndpointSettings();

        #endregion

        #region 由 request决定

        public ApiMethodName ApiMethodName { get; protected set; }

        public ApiRequestAuth Auth { get; protected set; }

        public string? Condition { get; set; }

        #endregion

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        protected HttpRequestBuilder(ApiMethodName apiMethodName, ApiRequestAuth auth, string? condition)
        {
            ApiMethodName = apiMethodName;
            Auth = auth;
            Condition = condition;
        }

        public void SetJwt(string jwt)
        {
            Headers[ApiHeaderNames.Authorization] = $"{EndpointSettings.Challenge} {jwt}";
        }

        public void SetApiKey(string apiKey)
        {
            Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public void SetDeviceId(string deviceId)
        {
            Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public void SetDeviceVersion(string deviceVersion)
        {
            Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

        public abstract string GetUrl();

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(Auth);
            hashCode.Add(ApiMethodName);
            hashCode.Add(Condition);

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }

            return hashCode.ToHashCode();
        }
    }
}