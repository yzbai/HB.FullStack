using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// POST /Model/ByBatch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AddByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public AddByBatchRequest() { }

        public AddByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethodName.Post, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}