
/* Unmerged change from project 'HB.FullStack.Common (netstandard2.1)'
Before:
using System.Net.Http;
After:
using System.Net.Http;
using HB;
using HB.FullStack;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Api.Requests;
*/
using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public enum ApiMethod
    {
        None = 0,
        Get = 1,
        Add = 2,
        Update = 3,
        Delete = 4,
        UpdateFields = 5
    }

    public static class ApiMethodExtensions
    {
        //NOTICE: UpdateFields not defined in .net standard 2.0
        //private static readonly HttpMethod _patch = new HttpMethod("UpdateFields");

        public static HttpMethod ToHttpMethod(this ApiMethod apiMethodName)
        {
            return apiMethodName switch
            {
                ApiMethod.None => throw new System.NotImplementedException(),
                ApiMethod.Get => HttpMethod.Get,
                ApiMethod.Add => HttpMethod.Post,
                ApiMethod.Update => HttpMethod.Put,
                ApiMethod.Delete => HttpMethod.Delete,
                ApiMethod.UpdateFields => HttpMethod.Patch,
                _ => throw new System.NotImplementedException(),
            };
        }

        public static string ToHttpMethodString(this ApiMethod httpMethodName)
        {
            return httpMethodName switch
            {
                ApiMethod.None => throw new System.NotImplementedException(),
                ApiMethod.Get => "Get",
                ApiMethod.Add => "Post",
                ApiMethod.Update => "Put",
                ApiMethod.Delete => "Delete",
                ApiMethod.UpdateFields => "Patch",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}