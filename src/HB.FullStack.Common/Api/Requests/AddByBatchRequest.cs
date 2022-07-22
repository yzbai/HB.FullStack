using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class AddByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public AddByBatchRequest() { }

        public AddByBatchRequest(IEnumerable<T> ress, ApiRequestAuth auth) : base(typeof(T).Name, ApiMethodName.Post, auth, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}