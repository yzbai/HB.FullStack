using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// POST /Model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AddRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [ValidatedObject(CanBeNull = false)]
        [RequestBody]
        public T Resource { get; set; } = null!;

        public AddRequest(T res) : base(typeof(T).Name, ApiMethod.Add, null, null)
        {
            Resource = res;
        }
    }
}