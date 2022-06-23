using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 除内容外构建Request需要的信息
    /// </summary>
    public abstract class HttpRequestBuilder
    {
        public HttpMethodName HttpMethod { get; protected set; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public ApiAuthType AuthType { get; internal set; }

        public string? ApiKeyName { get; set; }

        protected HttpRequestBuilder(HttpMethodName httpMethod, ApiAuthType apiAuthType, string? apiKeyName = null)
        {
            HttpMethod = httpMethod;
            AuthType = apiAuthType;
            ApiKeyName = apiKeyName;
        }

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

        public void SetDeviceVersion(string deviceVersion)
        {
            Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

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

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(AuthType);
            hashCode.Add(ApiKeyName);
            hashCode.Add(HttpMethod);

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }

            return hashCode.ToHashCode();
        }
    }
}