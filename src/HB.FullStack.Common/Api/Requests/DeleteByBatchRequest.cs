using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// DELETE /Model/ByBatch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeleteByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public DeleteByBatchRequest() { }

        public DeleteByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethodName.Delete, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}