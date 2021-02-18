using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;


namespace HB.FullStack.Common.Api
{
    public class UpdateRequest<T> : ApiRequest<T> where T : ApiResource
    {
        [IdBarrier]
        [CollectionNotEmpty]
        public IList<T> Resources { get; set; } = new List<T>();

        public UpdateRequest() : base(HttpMethod.Put, null) { }

        public UpdateRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Put, null) { }

        public UpdateRequest(IEnumerable<T> ress) : this()
        {
            Resources.AddRange(ress);
        }

        public UpdateRequest(string apiKeyName, IEnumerable<T> ress) : this(apiKeyName)
        {
            Resources.AddRange(ress);
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
            throw new NotSupportedException();
        }
    }
}