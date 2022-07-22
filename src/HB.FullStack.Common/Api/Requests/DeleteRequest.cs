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

        public DeleteRequest(T res, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Delete, auth, condition)
        {
            Resource = res;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Resource);
        }
    }
}