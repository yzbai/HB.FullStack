﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdsRequest<T> : ApiRequest where T : ApiResource2
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        [OnlyForJsonConstructor]
        public GetByIdsRequest()  { }

        public GetByIdsRequest(params Guid[] ids) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Get, true, ApiAuthType.Jwt, "ByIds"))
        {
            ids.AddRange(ids);
        }
    }
}