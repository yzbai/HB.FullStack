
/* Unmerged change from project 'HB.FullStack.Common (netstandard2.1)'
Before:
using System.Net.Http;
After:
using System.Net.Http;
using HB;
using HB.FullStack;
using HB.FullStack.Common;


using HB.FullStack.Common.Api.Requests;
*/
using System.Net.Http;

namespace HB.FullStack.Client.ApiClient
{
    public enum ApiMethod
    {
        None = 0,
        Get = 1,
        Add = 2,
        Update = 3,
        Delete = 4,
        UpdateProperties = 5,
        UpdateRelation = 6
    }

    public static class ApiMethodExtensions
    {
        //NOTICE: UpdateProperties not defined in .net standard 2.0
        //private static readonly HttpMethod _patch = new HttpMethod("UpdateProperties");

        public static HttpMethod ToHttpMethod(this ApiMethod apiMethodName)
        {
            return apiMethodName switch
            {
                ApiMethod.None => throw new System.NotImplementedException(),
                ApiMethod.Get => HttpMethod.Get,
                ApiMethod.Add => HttpMethod.Post,
                ApiMethod.Update => HttpMethod.Put,
                ApiMethod.Delete => HttpMethod.Delete,
                ApiMethod.UpdateProperties => HttpMethod.Patch,
                ApiMethod.UpdateRelation => HttpMethod.Patch,
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
                ApiMethod.UpdateProperties => "Patch",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}