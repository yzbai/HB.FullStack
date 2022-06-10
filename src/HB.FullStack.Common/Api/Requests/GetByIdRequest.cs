using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdRequest<T> : ApiRequest where T : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        [OnlyForJsonConstructor]
        public GetByIdRequest() { }

        public GetByIdRequest(Guid id) : base(new RestfulHttpRequestBuilder<T>(HttpMethodName.Get, true, "ById"))
        {
            Id = id;
        }
    }
}