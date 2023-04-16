using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Services.IdBarriers;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Client.ApiClient
{
    /// <summary>
    /// DELETE /Model/ByBatch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DeleteByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        [RequestBody]
        public IList<T> Resources { get; set; } = new List<T>();

        public DeleteByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethod.Delete, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}