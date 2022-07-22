using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PostRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [Required]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public PostRequest() { }

        public PostRequest(T res, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Post, auth, condition)
        {
            Resource = res;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Resource);
        }
    }
}