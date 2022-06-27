using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        [OnlyForJsonConstructor]
        public GetByIdsRequest() { }

        public GetByIdsRequest(ApiRequestAuth auth, params Guid[] ids) : base(ApiMethodName.Get, auth, "ByIds")
        {
            ids.AddRange(ids);
        }
    }
}