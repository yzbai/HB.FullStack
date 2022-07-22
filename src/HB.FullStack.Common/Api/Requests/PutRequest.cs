using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PutRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [Required]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public PutRequest() { }

        public PutRequest(T res, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Put, auth, condition)
        {
            Resource = res;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Resource.GetHashCode());
        }
    }
}