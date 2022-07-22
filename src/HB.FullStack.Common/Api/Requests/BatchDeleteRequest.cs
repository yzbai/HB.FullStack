using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class BatchDeleteRequest<T> : ApiRequest where T : ApiResource
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public BatchDeleteRequest() { }

        public BatchDeleteRequest(IEnumerable<T> ress, string resName, ApiRequestAuth auth, string? condition) : base(resName, ApiMethodName.Delete, auth, condition)
        {
            Resources.AddRange(ress);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(base.GetHashCode());

            foreach (T item in Resources)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}