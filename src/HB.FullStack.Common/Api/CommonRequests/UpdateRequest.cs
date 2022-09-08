using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public sealed class UpdateRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        [RequestBody]
        public T Resource { get; set; } = null!;

        public UpdateRequest(T res) : base(typeof(T).Name, ApiMethod.Update, null, null)
        {
            Resource = res;
        }
    }
}