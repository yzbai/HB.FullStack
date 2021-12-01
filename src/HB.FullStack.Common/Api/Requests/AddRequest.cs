using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; } = new List<T>();

        public AddRequest(IEnumerable<T> ress, Guid? ownerResId) : base(HttpMethodName.Post, null, ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(string apiKeyName, IEnumerable<T> ress, Guid? ownerResId) : base(apiKeyName, HttpMethodName.Post, null, ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(T res, Guid? ownerResId) : this(new T[] { res }, ownerResId) { }

        public AddRequest(string apiKeyName, T res, Guid? ownerResId) : this(apiKeyName, new T[] { res }, ownerResId) { }

        public override string ToDebugInfo()
        {
            return $"AddRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
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