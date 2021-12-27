using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
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