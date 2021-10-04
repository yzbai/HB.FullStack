using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [IdBarrier]
        [CollectionMemeberValidated]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        public UpdateRequest() : base(HttpMethod.Put, null) { }

        public UpdateRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Put, null) { }

        public UpdateRequest(IEnumerable<T> res) : this()
        {
            Resources.AddRange(res);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> res) : this(apiKeyName)
        {
            Resources.AddRange(res);
        }

        public UpdateRequest(T res) : this()
        {
            Resources.Add(res);
        }

        public UpdateRequest(string apiKeyName, T res) : this(apiKeyName)
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
            return $"UpdateRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }
    }
}