using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        public DeleteRequest(IEnumerable<T> ress, Guid? ownerResId) : base(HttpMethodName.Delete, null, ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, IEnumerable<T> ress,Guid? ownerResId) : base(apiKeyName, HttpMethodName.Delete,null ,ownerResId, null)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res, Guid? ownerResId) : this(new T[] { res }, ownerResId) { }

        public DeleteRequest(string apiKeyName, T res, Guid? ownerResId) : this(apiKeyName, new T[] { res }, ownerResId) { }

        public override string ToDebugInfo()
        {
            return $"DeleteRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
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