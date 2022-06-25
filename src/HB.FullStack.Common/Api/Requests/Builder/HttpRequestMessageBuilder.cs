using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 一个Request包含两部分：1，业务内容；2，http构建信息
    /// 除内容外构建Request需要的信息
    /// </summary>
    public abstract class HttpRequestMessageBuilder
    {
        public HttpMethodName HttpMethodName { get; protected set; }

        public HttpMethodOverrideMode HttpMethodOverrideMode { get; protected set; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public ApiAuthType AuthType { get; internal set; }

        public string? ApiKeyName { get; set; }

        protected HttpRequestMessageBuilder(HttpMethodName httpMethodName, HttpMethodOverrideMode httpMethodOverrideMode, ApiAuthType apiAuthType, string? apiKeyName = null)
        {
            HttpMethodName = httpMethodName;
            HttpMethodOverrideMode = httpMethodOverrideMode;
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

        public abstract string GetUrl();

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();

            hashCode.Add(AuthType);
            hashCode.Add(ApiKeyName);
            hashCode.Add(HttpMethodName);

            foreach (KeyValuePair<string, string> kv in Headers)
            {
                hashCode.Add(kv.Key);
                hashCode.Add(kv.Value);
            }

            return hashCode.ToHashCode();
        }
    }

    public static class HttpRequestMessageBuilderExtensions
    {
        //private static readonly Version _version20 = new Version(2, 0);

        /// <summary>
        /// 构建HTTP的基本信息
        /// 之所以写成扩展方法的形式，是为了避免HttpRequestBuilder过大。又为了调用方式比静态方法舒服。
        /// </summary>
        public static HttpRequestMessage Build(this HttpRequestMessageBuilder builder, ApiRequest apiRequest)
        {
            //1. Mthod
            HttpMethod httpMethod = builder.HttpMethodName.ToHttpMethod();

            switch (builder.HttpMethodOverrideMode)
            {
                case HttpMethodOverrideMode.None:
                    break;
                case HttpMethodOverrideMode.Normal:
                    if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Post)
                    {
                        builder.Headers["X-HTTP-Method-Override"] = httpMethod.Method;
                        httpMethod = HttpMethod.Post;
                    }
                    break;
                case HttpMethodOverrideMode.All:
                    builder.Headers["X-HTTP-Method-Override"] = httpMethod.Method;
                    httpMethod = HttpMethod.Post;
                    break;
            }

            //2. url
            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, AssembleUrl(builder))
            {
                //TODO: 看需要不需要使用http2.0
                //Version = _version20
            };

            //3. headers
            foreach (var kv in builder.Headers)
            {
                httpRequest.Headers.Add(kv.Key, kv.Value);
            }

            //4, contents
            if (apiRequest is IUploadRequest fileUpdateRequest)
            {
                httpRequest.Content = BuildMultipartContent(fileUpdateRequest);
            }
            else if (httpRequest.Method == HttpMethod.Get)
            {
                //TODO: Implement this
                throw new NotImplementedException("还没有实现Http Get 把参数都放到Query中去，请先使用HttpMethodOverrideMode=ALL");
            }
            else
            {
                //具体传递的数据
                //包括Get的参数也放到body中去
                httpRequest.Content = new StringContent(SerializeUtil.ToJson(apiRequest), Encoding.UTF8, "application/json");
            }

            return httpRequest;

            static string AssembleUrl(HttpRequestMessageBuilder builder)
            {
                string uri = builder.GetUrl();

                IDictionary<string, string?> parameters = new Dictionary<string, string?>
                {
                    { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                    { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)}
                };

                return UriUtil.AddQuerys(uri, parameters);
            }

            [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "关于Dispose：MultipartFormDataContent Dipose的时候，会把子content Dipose掉。 而HttpRequestMessage Dispose的时候，会把他的Content Dispose掉")]
            static MultipartFormDataContent BuildMultipartContent(IUploadRequest fileRequest)
            {
                MultipartFormDataContent content = new MultipartFormDataContent();

                string httpContentName = fileRequest.HttpContentName;
                byte[] file = fileRequest.GetFile();
                string fileName = fileRequest.FileName;

                ByteArrayContent byteArrayContent = new ByteArrayContent(file);
                content.Add(byteArrayContent, httpContentName, fileName);

                content.Add(new StringContent(SerializeUtil.ToJson(fileRequest), Encoding.UTF8, "application/json"));

                return content;
            }
        }
    }
}