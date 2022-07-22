using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// DELETE /Model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeleteRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public DeleteRequest() { }

        public DeleteRequest(T res) : base(typeof(T).Name, ApiMethodName.Delete, null, null)
        {
            Resource = res;
        }
    }
}