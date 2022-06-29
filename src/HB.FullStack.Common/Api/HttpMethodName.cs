using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public enum ApiMethodName
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
        //private static readonly HttpMethod _patch = new HttpMethod("Patch");

        public static HttpMethod ToHttpMethod(this ApiMethodName apiMethodName)
        {
            return apiMethodName switch
            {
                ApiMethodName.None => throw new System.NotImplementedException(),
                ApiMethodName.Get => HttpMethod.Get,
                ApiMethodName.Post => HttpMethod.Post,
                ApiMethodName.Put => HttpMethod.Put,
                ApiMethodName.Delete => HttpMethod.Delete,
                ApiMethodName.Patch => HttpMethod.Patch,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static string ToHttpMethodString(this ApiMethodName httpMethodName)
        {
            return httpMethodName switch
            {
                ApiMethodName.None => throw new System.NotImplementedException(),
                ApiMethodName.Get => "Get",
                ApiMethodName.Post => "Post",
                ApiMethodName.Put => "Put",
                ApiMethodName.Delete => "Delete",
                ApiMethodName.Patch => "Patch",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}