using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public UpdateRequest() { }

        public UpdateRequest(T res, ApiRequestAuth auth, string? condition) : base(typeof(T).Name, ApiMethodName.Put, auth, condition)
        {
            Resource = res;
        }
    }
}