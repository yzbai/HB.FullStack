using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public sealed class UpdateByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public UpdateByBatchRequest() { }

        public UpdateByBatchRequest(IEnumerable<T> ress) : base(typeof(T).Name, ApiMethod.Update, null, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}