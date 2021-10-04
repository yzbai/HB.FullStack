using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : ApiResource2
    {
        [CollectionNotEmpty]
        [CollectionMemeberValidated]
        [IdBarrier]
        public IList<T> Resources { get; set; } = new List<T>();

        public AddRequest() : base(HttpMethod.Post, null) { }

        public AddRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Post, null) { }

        public AddRequest(IEnumerable<T> ress) : this()
        {
            Resources.AddRange(ress);
        }

        public AddRequest(string apiKeyName, IEnumerable<T> ress) : this(apiKeyName)
        {
            Resources.AddRange(ress);
        }

        public AddRequest(T res) : this()
        {
            Resources.Add(res);
        }

        public AddRequest(string apiKeyName, T res) : this(apiKeyName)
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
            return $"AddRequest, ApiResourceType:{typeof(T).Name}, Resources:{SerializeUtil.ToJson(Resources)}";
        }
    }

}