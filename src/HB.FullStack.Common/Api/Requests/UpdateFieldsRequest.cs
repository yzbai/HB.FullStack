using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// 更新几个字段
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateFieldsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        protected UpdateFieldsRequest(string? condition) : base(HttpMethodName.Patch, condition) { }

        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(apiKeyName, HttpMethodName.Patch, condition) { }
    }
}