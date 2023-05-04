using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Components.IdBarriers;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    public sealed class UpdateByBatchRequest<T> : ApiRequest where T : SharedResource
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