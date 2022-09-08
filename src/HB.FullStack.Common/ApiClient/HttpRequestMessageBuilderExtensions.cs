using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public static class HttpRequestMessageBuilderExtensions
    {
        public static void SetJwt(this HttpRequestMessageBuilder builder, string jwt)
        {
            builder.Headers[ApiHeaderNames.Authorization] = $"{builder.SiteSetting.Challenge} {jwt}";
        }

        public static void SetApiKey(this HttpRequestMessageBuilder builder, string apiKey)
        {
            builder.Headers[ApiHeaderNames.XApiKey] = apiKey;
        }

        public static void SetDeviceId(this HttpRequestMessageBuilder builder, string deviceId)
        {
            builder.Headers[ApiHeaderNames.DEVICE_ID] = deviceId;
        }

        public static void SetDeviceVersion(this HttpRequestMessageBuilder builder, string deviceVersion)
        {
            builder.Headers[ApiHeaderNames.DEVICE_VERSION] = deviceVersion;
        }

        /// <summary>
        /// 构建HTTP的基本信息
        /// 之所以写成扩展方法的形式，是为了避免HttpRequestBuilder过大。又为了调用方式比静态方法舒服。
        /// </summary>
        public static HttpRequestMessage Build(this HttpRequestMessageBuilder builder)
        {
            //1. Mthod
            HttpMethod httpMethod = builder.Request.ApiMethod.ToHttpMethod();

            switch (builder.SiteSetting.HttpMethodOverrideMode)
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
            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, builder.GenerateUrl())
            {
                Version = builder.SiteSetting.HttpVersion
            };

            //3. headers
            foreach (var kv in builder.Headers)
            {
                httpRequest.Headers.Add(kv.Key, kv.Value);
            }

            //4, contents
            if (builder.Request is IUploadRequest uploadRequest)
            {
                httpRequest.Content = BuildMultipartContent(uploadRequest);
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
                httpRequest.Content = new StringContent(SerializeUtil.ToJson(builder.Request), Encoding.UTF8, "application/json");
            }

            return httpRequest;
        }

        public static string GenerateUrl(this HttpRequestMessageBuilder httpRequestBuilder)
        {
            List<KeyValuePair<string, string>> queryParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) ),
                new KeyValuePair<string, string>(ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture))
            };

            if (httpRequestBuilder.ResEndpoint.Type == ResEndpointType.PlainUrl)
            {
                return UriUtil.AddQuerys(httpRequestBuilder.ResEndpoint.ControllerOrPlainUrl, queryParameters);
            }

            if (httpRequestBuilder.ResEndpoint.Type != ResEndpointType.ControllerModel)
            {
                throw new NotImplementedException("Other ResEndpointType not implemented.");
            }

            StringBuilder uriBuilder = new StringBuilder();

            //Version
            if (httpRequestBuilder.SiteSetting.Version.IsNotNullOrEmpty())
            {
                uriBuilder.Append(httpRequestBuilder.SiteSetting.Version);
                uriBuilder.Append('/');
            }

            //ControllerModelName
            uriBuilder.Append(httpRequestBuilder.ResEndpoint.ControllerOrPlainUrl);

            //Condition
            if (httpRequestBuilder.Request.Condition.IsNotNullOrEmpty())
            {
                uriBuilder.Append('/');
                uriBuilder.Append(httpRequestBuilder.Request.Condition);
            }

            string uri = uriBuilder.ToString();

            //Queries
            //将ApiRequest中标记RequestQueryAttribute的属性放到queryParameters中

            ApiRequest request = httpRequestBuilder.Request;



            return UriUtil.AddQuerys(uri, queryParameters);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "关于Dispose：MultipartFormDataContent Dipose的时候，会把子content Dipose掉。 而HttpRequestMessage Dispose的时候，会把他的Content Dispose掉")]
        private static MultipartFormDataContent BuildMultipartContent(IUploadRequest fileRequest)
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