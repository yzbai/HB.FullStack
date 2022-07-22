using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PutRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public PutRequest() { }

        public PutRequest(T res, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Put, auth, condition)
        {
            Resource = res;
        }
    }
}