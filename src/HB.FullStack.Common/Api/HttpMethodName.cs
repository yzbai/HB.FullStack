using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public enum HttpMethodName
    {
        None = 0,
        Get = 1,
        Post = 2,
        Put = 3,
        Delete = 4,
        Patch = 5
    }

    public static class HttpMethodNameExtensions
    {
        //NOTICE: Patch not defined in .net standard 2.0
        private static readonly HttpMethod _patch = new HttpMethod("Patch");

        public static HttpMethod ToHttpMethod(this HttpMethodName httpMethodName)
        {
            return httpMethodName switch
            {
                HttpMethodName.None => throw new System.NotImplementedException(),
                HttpMethodName.Get => HttpMethod.Get,
                HttpMethodName.Post => HttpMethod.Post,
                HttpMethodName.Put => HttpMethod.Put,
                HttpMethodName.Delete => HttpMethod.Delete,
                HttpMethodName.Patch => _patch,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static string ToHttpMethodString(this HttpMethodName httpMethodName)
        {
            return httpMethodName switch
            {
                HttpMethodName.None => throw new System.NotImplementedException(),
                HttpMethodName.Get => "Get",
                HttpMethodName.Post => "Post",
                HttpMethodName.Put => "Put",
                HttpMethodName.Delete => "Delete",
                HttpMethodName.Patch => "Patch",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}