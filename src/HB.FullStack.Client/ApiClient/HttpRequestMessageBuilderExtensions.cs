using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;

using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.ApiClient
{
    public static class HttpRequestMessageBuilderExtensions
    {
        public static void SetJwt(this HttpRequestMessageBuilder builder, string jwt)
        {
            builder.Headers[SharedNames.ApiHeaders.Authorization] = $"{builder.SiteSetting.Challenge} {jwt}";
        }

        public static void SetApiKey(this HttpRequestMessageBuilder builder, string apiKey)
        {
            builder.Headers[SharedNames.ApiHeaders.XApiKey] = apiKey;
        }

        public static void SetClientId(this HttpRequestMessageBuilder builder, string clientId)
        {
            builder.Headers[SharedNames.ApiHeaders.CLIENT_ID] = clientId;
        }

        public static void SetClientVersion(this HttpRequestMessageBuilder builder, string clientVersion)
        {
            builder.Headers[SharedNames.ApiHeaders.CLIENT_VERSION] = clientVersion;
        }

        /// <summary>
        /// 构建HTTP的基本信息
        /// 之所以写成扩展方法的形式，是为了避免HttpRequestBuilder过大。又为了调用方式比静态方法舒服。
        /// </summary>
        public static HttpRequestMessage Build(this HttpRequestMessageBuilder builder)
        {
            //1. Mthod
            HttpMethod httpMethod = builder.Request.ApiMethod.ToHttpMethod();

            if (builder.SiteSetting.UseHttpMethodOverride && httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Post)
            {
                builder.Headers["X-HTTP-Method-Override"] = httpMethod.Method;
                httpMethod = HttpMethod.Post;
            }

            //2. url
            HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, builder.BuildUriString())
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
            else
            {
                httpRequest.Content = BuildContent(builder.Request);
            }

            return httpRequest;
        }

        public static string BuildUriString(this HttpRequestMessageBuilder httpRequestBuilder)
        {

            if (httpRequestBuilder.ResEndpoint.Type == ResEndpointType.PlainUrl)
            {
                return UriUtil.AddQueryString(httpRequestBuilder.ResEndpoint.ControllerOrPlainUrl, UriUtil.NoiseQueryString);
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

            //Queries
            string? query = httpRequestBuilder.Request.BuildQueryString();

            if (query.IsNotNullOrEmpty())
            {
                uriBuilder.Append('?');
                uriBuilder.Append(query);
                uriBuilder.Append('&');
                uriBuilder.Append(UriUtil.NoiseQueryString);
            }
            else
            {
                uriBuilder.Append('?');
                uriBuilder.Append(UriUtil.NoiseQueryString);
            }

            return uriBuilder.ToString();
        }

        private static HttpContent BuildContent(ApiRequest request)
        {
            object? requestBody = request.GetRequestBody();

            return new StringContent(SerializeUtil.ToJson(requestBody) ?? String.Empty, Encoding.UTF8, "application/json");
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