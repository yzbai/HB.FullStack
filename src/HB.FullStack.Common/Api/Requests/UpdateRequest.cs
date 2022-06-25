using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public UpdateRequest() { }

        public UpdateRequest(IEnumerable<T> ress, string? condition) : base(HttpMethodName.Put, condition)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res, string? condition) : this(new T[] { res }, condition) { }

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