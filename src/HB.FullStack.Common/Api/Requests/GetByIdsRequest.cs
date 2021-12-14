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

    public class GetByIdRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        public GetByIdRequest() : base(HttpMethodName.Get, "ById") { }

        public GetByIdRequest(Guid id) : this()
        {
            Id = id;
        }

        public override string ToDebugInfo()
        {
            return $"GetByIdRequest. {GetType().FullName}";
        }
    }
}