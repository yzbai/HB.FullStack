using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public DeleteRequest() { }

        public DeleteRequest(T res, ApiRequestAuth auth) : base(typeof(T).Name, ApiMethodName.Delete, auth, null)
        {
            Resource = res;
        }
    }
}