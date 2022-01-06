﻿using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public static class HttpMethodNameExtensions
    {
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
    }
}