using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class UpdateByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public UpdateByBatchRequest() { }

        public UpdateByBatchRequest(IEnumerable<T> ress, ApiRequestAuth auth) : base(typeof(T).Name, ApiMethodName.Put, auth, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}