using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdRequest<T> : ApiRequest where T : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        public GetByIdRequest() : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Get, true, ApiAuthType.Jwt, "ById")) { }

        public GetByIdRequest(Guid id) : this()
        {
            Id = id;
        }
    }
}