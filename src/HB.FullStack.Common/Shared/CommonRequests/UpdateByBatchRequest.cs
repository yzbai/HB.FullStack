using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared.Attributes;

namespace HB.FullStack.Common.Shared.CommonRequests
{
    public sealed class UpdateByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        [RequestBody]
        public IList<T> Resources { get; set; } = new List<T>();

        public UpdateByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethod.Update, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}