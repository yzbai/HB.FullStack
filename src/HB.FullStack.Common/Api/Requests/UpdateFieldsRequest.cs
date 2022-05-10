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
    public abstract class UpdateFieldsRequest<T> : ApiRequest where T : ApiResource2
    {
        [OnlyForJsonConstructor]
        protected UpdateFieldsRequest() { }

        protected UpdateFieldsRequest(string? condition) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Patch, true, ApiAuthType.Jwt, condition)) { }

        protected UpdateFieldsRequest(string apiKeyName, string? condition) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Patch, true, apiKeyName, condition)) { }
    }
}