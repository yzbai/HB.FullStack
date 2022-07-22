using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public AddRequest() { }

        public AddRequest(T res, ApiRequestAuth auth, string? condition) : base(typeof(T).Name, ApiMethodName.Post, auth, condition)
        {
            Resource = res;
        }
    }
}