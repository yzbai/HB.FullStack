using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PutByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public PutByBatchRequest() { }

        public PutByBatchRequest(IEnumerable<T> ress, string resName, ApiRequestAuth auth) : base(resName, ApiMethodName.Put, auth, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}