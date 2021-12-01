using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    /// <summary>
    /// PUT /Ver/ResoruceCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        public UpdateRequest(IEnumerable<T> ress, Guid? ownerResId) : base(HttpMethodName.Put, null, ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> ress, Guid? ownerResId) : base(apiKeyName, HttpMethodName.Put, null, ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(T res, Guid? ownerResId) : this(new T[] { res }, ownerResId) { }

        public UpdateRequest(string apiKeyName, T res, Guid? ownerResId) : this(apiKeyName, new T[] { res }, ownerResId) { }

        public override string ToDebugInfo()
        {
            return $"UpdateRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
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