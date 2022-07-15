using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api.Requests;


namespace HB.FullStack.Common.Api
{
    public sealed class DeleteRequest<T> : ApiRequest<T> where T : ApiResource
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        [OnlyForJsonConstructor]
        public DeleteRequest() { }

        public DeleteRequest(IEnumerable<T> ress, ApiRequestAuth auth, string? condition) : base(ApiMethodName.Delete, auth, condition)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res, ApiRequestAuth auth, string? condition) : this(new T[] { res }, auth, condition) { }

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