using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdRequest<T> : GetRequest<T> where T : ApiResource
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        [OnlyForJsonConstructor]
        public GetByIdRequest() { }

        public GetByIdRequest(Guid id, ApiRequestAuth auth) : base(auth, "ById")
        {
            Id = id;
        }
    }
}