using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Common.Api
{
    public class AddRequest<T> : ApiRequest<T> where T : Resource
    {
        [CollectionNotEmpty]
        [IdBarrier]
        public List<T> Resources { get; set; } = new List<T>();

        protected AddRequest() : base(HttpMethod.Post, null) { }

        protected AddRequest(string apiKeyName) : base(apiKeyName, HttpMethod.Post, null) { }

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
            return HashCode.Combine(Resources);
        }
    }

}