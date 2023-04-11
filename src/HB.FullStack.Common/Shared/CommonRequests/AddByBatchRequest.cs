using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.CommonRequests
{
    /// <summary>
    /// POST /Model/ByBatch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AddByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        [RequestBody]
        public IList<T> Resources { get; set; } = new List<T>();

        public AddByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethod.Add, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}