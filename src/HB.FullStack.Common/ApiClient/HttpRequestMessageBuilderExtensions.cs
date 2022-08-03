﻿using System;
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
            builder.Headers[ApiHeaderNames.Authorization] = $"{builder.EndpointSetting.Challenge} {jwt}";
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
            HttpMethod httpMethod = builder.Request.ApiMethodName.ToHttpMethod();

            switch (builder.ResBinding.EndpointSetting!.HttpMethodOverrideMode)
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
                Version = builder.ResBinding.EndpointSetting.HttpVersion
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

        public static string GenerateUrlCore(this HttpRequestMessageBuilder httpRequestBuilder)
        {
            if (httpRequestBuilder.ResBinding.Type == ResBindingType.PlainUrl)
            {
                return httpRequestBuilder.ResBinding.BindingValue;
            }

            if (httpRequestBuilder.ResBinding.Type != ResBindingType.ControllerModel)
            {
                throw new NotImplementedException("Other ResBindingType not implemented.");
            }

            StringBuilder builder = new StringBuilder();

            //Version
            if (httpRequestBuilder.ResBinding.EndpointSetting!.Version.IsNotNullOrEmpty())
            {
                builder.Append(httpRequestBuilder.ResBinding.EndpointSetting.Version);
                builder.Append('/');
            }

            //ControllerModelName
            builder.Append(httpRequestBuilder.ResBinding.BindingValue);

            //Condition
            if (httpRequestBuilder.Request.Condition.IsNotNullOrEmpty())
            {
                builder.Append('/');
                builder.Append(httpRequestBuilder.Request.Condition);
            }

            //Query
            if(httpRequestBuilder.EndpointSetting.HttpMethodOverrideMode == HttpMethodOverrideMode.All)
            {
                return builder.ToString();
            }

            return builder.ToString();
        }

        public static string GenerateUrl(this HttpRequestMessageBuilder builder)
        {
            string uri = builder.GenerateUrlCore();

            IDictionary<string, string?> parameters = new Dictionary<string, string?>
                {
                    { ClientNames.RANDOM_STR, SecurityUtil.CreateRandomString(6) },
                    { ClientNames.TIMESTAMP, TimeUtil.UtcNowUnixTimeMilliseconds.ToString(CultureInfo.InvariantCulture)}
                };

            return UriUtil.AddQuerys(uri, parameters);
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