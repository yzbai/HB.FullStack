using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api.Requests
{
    public class GetByIdRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [NoEmptyGuid]
        public Guid Id { get; set; }

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public GetByIdRequest()
        { }

        public GetByIdRequest(Guid id) : base(HttpMethodName.Get, "ById")
        {
            Id = id;
        }
    }
}