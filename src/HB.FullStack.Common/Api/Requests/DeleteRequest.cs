using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public class DeleteRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        public DeleteRequest() : base(HttpMethod.Delete, null) { }

        public DeleteRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Delete, null) { }

        public DeleteRequest(IEnumerable<T> ress) : this()
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(string apiKeyName, IEnumerable<T> ress) : this(apiKeyName)
        {
            Resources.AddRange(ress);
        }

        public DeleteRequest(T res) : this()
        {
            Resources.Add(res);
        }

        public DeleteRequest(string apiKeyName, T res) : this(apiKeyName)
        {
            Resources.Add(res);
        }

        public void AddResource(params T[] ress)
        {
            Resources.AddRange(ress);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType().Name, Resources);
        }

        public override string ToDebugInfo()
        {
            return $"DeleteRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }
    }
}