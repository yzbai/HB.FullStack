using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;

namespace HB.FullStack.Common.Api
{
    public static class ApiRequestExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this ApiRequest request)
        {
            //step1: 创建http基本信息
            HttpRequestMessage httpRequest = request.RequestBuilder!.Build();

            //step2: 填充Content，具体数据

            //具体传递的数据
            //包括Get的参数也放到body中去
            httpRequest.Content = new StringContent(SerializeUtil.ToJson(request), Encoding.UTF8, "application/json");

            if (request is IUploadRequest fileUpdateRequest)
            {
                httpRequest.Content = BuildMultipartContent(fileUpdateRequest);
            }

            return httpRequest;

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