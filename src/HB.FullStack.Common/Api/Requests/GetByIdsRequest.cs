using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdsRequest<T> : ApiRequest where T : ApiResource2
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        public GetByIdsRequest() : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Get, true, ApiAuthType.Jwt, "ByIds")) { }

        public GetByIdsRequest(params Guid[] ids) : this()
        {
            ids.AddRange(ids);
        }
    }
}