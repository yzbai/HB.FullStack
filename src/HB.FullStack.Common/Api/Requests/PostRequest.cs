using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PostRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public PostRequest() { }

        public PostRequest(T res, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Post, auth, condition)
        {
            Resource = res;
        }
    }
}