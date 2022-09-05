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
        public T Resource { get; set; } = null!;

        [OnlyForJsonConstructor]
        public AddRequest() { }

        public AddRequest(T res) : base(typeof(T).Name, ApiMethod.Add, null, null)
        {
            Resource = res;
        }
    }
}