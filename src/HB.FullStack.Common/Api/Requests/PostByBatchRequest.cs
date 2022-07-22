using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class PostByBatchRequest<T> : ApiRequest where T : ApiResource
    {
        [IdBarrier]
        [CollectionMemeberValidated(CanBeNullOrEmpty = false)]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public PostByBatchRequest() { }

        public PostByBatchRequest(IEnumerable<T> ress, string resName, ApiRequestAuth auth) : base(resName, ApiMethodName.Post, auth, "ByBatch")
        {
            Resources.AddRange(ress);
        }
    }
}