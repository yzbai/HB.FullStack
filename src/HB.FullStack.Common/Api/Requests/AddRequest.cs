using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        [OnlyForJsonConstructor]
        public AddRequest() { }

        public AddRequest(IEnumerable<T> ress, ApiRequestAuth auth, string? condition) : base(HttpMethodName.Post, auth, condition)
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

        protected sealed override ApiRequestBuilder CreateBuilder()
        {
            return new RestfulApiRequestBuilder<T>(this);
        }
    }
}