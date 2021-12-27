using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdsRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [NoEmptyGuid]
        public IList<Guid> Ids { get; set; } = new List<Guid>();

        public GetByIdsRequest() : base(HttpMethodName.Get, "ByIds") { }

        public GetByIdsRequest(params Guid[] ids) : this()
        {
            ids.AddRange(ids);
        }

        public override string ToDebugInfo()
        {
            return $"GetByIdsRequest. {GetType().FullName}";
        }
    }
}