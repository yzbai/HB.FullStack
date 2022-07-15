using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api.Requests;


namespace HB.FullStack.Common.Api
{
    public sealed class AddRequest<T> : ApiRequest<T> where T : ApiResource
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        [OnlyForJsonConstructor]
        public AddRequest() { }

        public AddRequest(IEnumerable<T> ress, ApiRequestAuth auth, string? condition) : base(ApiMethodName.Post, auth, condition)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(T res, ApiRequestAuth auth, string? condition) : this(new T[] { res }, auth, condition) { }

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