using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdRequest<T> : ApiRequest<T> where T : ApiResource
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        [OnlyForJsonConstructor]
        public GetByIdRequest() { }

        public GetByIdRequest(Guid id, ApiRequestAuth auth) : base(ApiMethodName.Get, auth, "ById")
        {
            Id = id;
        }
    }
}