﻿using System;
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

        protected UpdateFieldsRequest(ApiRequestAuth auth, string? condition) : base(HttpMethodName.Patch, auth, condition) { }

        protected sealed override ApiRequestBuilder CreateBuilder()
        {
            return new RestfulApiRequestBuilder<T>(this);
        }
    }
}